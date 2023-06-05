using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using ArcadeBot.Net.Websockets.Models;
using Microsoft.Extensions.Logging;

namespace ArcadeBot.Net.Websockets;

internal partial class DiscordWebsocketClient : IDiscordWebSocketClient
{
    private readonly ILogger<DiscordWebsocketClient> _logger;
    private readonly AsyncLock _lock = new();
    private readonly Func<Uri, CancellationToken, Task<WebSocket>> _connectionFactory;
    private readonly Subject<SocketResponse> _messageReceivedSubject = new();
    private readonly Subject<ReconnectionInfo> _reconnectionSubject = new();
    private readonly Subject<DisconnectionInfo> _disconnectedSubject = new();

    private Timer? _lastChanceTimer;
    private DateTime _lastReceivedMsg = DateTime.UtcNow;

    private bool _reconnecting;
    private bool _stopping;
    private bool _isReconnectionEnabled = true;
    private WebSocket? _client;
    private CancellationTokenSource? _cancellation;
    private CancellationTokenSource? _cancellationTotal;

    public Uri Url { get; set; }

    public IObservable<SocketResponse> MessageReceived => _messageReceivedSubject.AsObservable();

    public IObservable<ReconnectionInfo> ReconnectionHappened => _reconnectionSubject.AsObservable();

    public IObservable<DisconnectionInfo> DisconnectionHappened => _disconnectedSubject.AsObservable();

    public TimeSpan? ReconnectTimeout { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan? ErrorReconnectTimeout { get; set; } = TimeSpan.FromMinutes(1);
    public Encoding? MessageEncoding { get; set; }

    public bool IsStarted { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsTextMessageConversionEnabled { get; set; } = true;

    public bool IsReconnectionEnabled
    {
        get => _isReconnectionEnabled; set
        {
            _isReconnectionEnabled = value;
            if (IsStarted)
            {
                if (_isReconnectionEnabled)
                    ActivateLastChance();
                else
                    DeactivateLastChance();


            }
        }
    }

    public ClientWebSocket NativeClient => GetSpecificOrThrow(_client);


    public DiscordWebsocketClient(ILogger<DiscordWebsocketClient> logger, Uri url, Func<ClientWebSocket>? clientFactory = null) : this(logger, url, GetFactory(clientFactory)) { }
    public DiscordWebsocketClient(ILogger<DiscordWebsocketClient> logger, Uri url, Func<Uri, CancellationToken, Task<WebSocket>>? connectionFactory)
    {
        _logger = logger;
        Url = url;
        _connectionFactory = connectionFactory ?? (async (uri, token) =>
        {
            var client = new ClientWebSocket();
            await client.ConnectAsync(uri, token).ConfigureAwait(false);
            return client;
        });
    }

    #region Start
    public Task Start() => StartInternal(false);
    public Task StartOrFail() => StartInternal(true);
    private async Task StartInternal(bool fastFail)
    {
        if (_disposed)
            throw new InvalidOperationException("The Client has already been disposed");
        if (IsStarted)
        {
            _logger.LogDebug("Client is already running, ignored"); return;
        }
        IsStarted = true;
        _logger.LogDebug("Starting client");
        _cancellation = new();
        _cancellationTotal = new();
        await StartClient(Url, ReconnectionType.Initial, fastFail, _cancellation.Token).ConfigureAwait(false);
        StartTextSendThread();
        StartBinarySendThread();
    }

    #endregion

    #region Stop
    public async Task<bool> Stop(WebSocketCloseStatus status, string statusDescription)
    {
        var result = await StopInternal(
            _client,
            status,
            statusDescription,
            false,
            false, null).ConfigureAwait(false);
        _disconnectedSubject.OnNext(DisconnectionInfo.Create(DisconnectionType.ByUser, _client, null));
        return result;
    }

    public async Task<bool> StopOrFail(WebSocketCloseStatus status, string statusDescription)
    {
        var result = await StopInternal(
            _client,
            status,
            statusDescription,
            true,
            false, null).ConfigureAwait(false);
        _disconnectedSubject.OnNext(DisconnectionInfo.Create(DisconnectionType.ByUser, _client, null));
        return result;
    }

    private async Task<bool> StopInternal(WebSocket client, WebSocketCloseStatus status, string statusDescription,
            bool failFast, bool byServer, CancellationToken? cancellation)
    {
        if (_disposed)
            throw new InvalidOperationException("The Client has already been disposed");
        DeactivateLastChance();
        if (client == null)
        {
            IsStarted = false;
            IsRunning = false;
            return false;
        }
        if (!IsRunning)
        {
            _logger.LogInformation("Client is already stopped");
            IsStarted = false;
            return false;
        }
        var result = false;
        try
        {
            var cancellationToken = cancellation ?? CancellationToken.None;
            _stopping = true;
            if (byServer)
                await client.CloseOutputAsync(status, statusDescription, cancellationToken);
            else
                await client.CloseAsync(status, statusDescription, cancellationToken);
            result = true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while stopping client");

            if (failFast)
            {
                // fail fast, propagate exception
                throw new WebSocketException("Failed to stop Websocket client, see inner exception", e);
            }
        }
        finally
        {
            IsRunning = false;
            _stopping = false;

            if (!byServer || !IsReconnectionEnabled)
            {
                // stopped manually or no reconnection, mark client as non-started
                IsStarted = false;
            }
        }

        return result;
    }

    #endregion

    private async Task StartClient(Uri url, ReconnectionType reconnType, bool fastFail, CancellationToken cancelToken)
    {
        DeactivateLastChance();
        try
        {
            _client = await _connectionFactory(url, cancelToken).ConfigureAwait(false);
            _ = Listen(_client, cancelToken);
            IsRunning = true;
            IsStarted = true;
            _reconnectionSubject.OnNext(ReconnectionInfo.Create(reconnType));
            _lastReceivedMsg = DateTime.UtcNow;
            ActivateLastChance();
        }
        catch (Exception e)
        {
            var info = DisconnectionInfo.Create(DisconnectionType.Error, _client, e);
            _disconnectedSubject.OnNext(info);
            if (info.CancelReconnection)
            {
                _logger.LogError("Exception while connecting. Canceled by user, exiting. Error: {message}", e.Message);
                return;
            }
            if (fastFail)
                throw new WebSocketException("Failed to start Websocket, see inner exception", e);
            if (ErrorReconnectTimeout is null)
            {
                _logger.LogError("Exception while connecting. Reconnecting disabled. Error: {message}", e.Message);
                return;
            }
            _logger.LogError("Exception while connecting. Reconnecting after {totalSeconds} seconds. Error: {message}", e.Message, ErrorReconnectTimeout?.TotalSeconds);
            await Task.Delay(ErrorReconnectTimeout!.Value, cancelToken).ConfigureAwait(false);
            await Reconnect(ReconnectionType.Error, false, e).ConfigureAwait(false);
        }
    }


    private async Task Listen(WebSocket client, CancellationToken token)
    { }











    private static DisconnectionType TranslateTypeToDisconnection(ReconnectionType type)
    {
        // beware enum indexes must correspond to each other
        return (DisconnectionType)type;
    }
    private static Func<Uri, CancellationToken, Task<WebSocket>>? GetFactory(Func<ClientWebSocket>? clientFactory)
    {
        if (clientFactory == null)
            return null;

        return async (uri, token) =>
        {
            var client = clientFactory();
            await client.ConnectAsync(uri, token).ConfigureAwait(false);
            return client;
        };
    }
    private static ClientWebSocket GetSpecificOrThrow(WebSocket? client)
    {
        if (client is null || client is not ClientWebSocket specific)
            throw new WebSocketException("Cannot cast 'WebSocket' client to 'ClientWebSocket', " +
                                         "provide correct type via factory or don't use this property at all.");
        return specific;
    }



    #region IDisposable
    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~DiscordWebsocketClient()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
