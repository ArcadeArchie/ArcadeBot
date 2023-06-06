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

/// <summary>
/// A simple websocket client with built-in reconnection and error handling
/// </summary>
public partial class DiscordWebsocketClient : IDiscordWebSocketClient
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
    private Encoding? _msgEncoding = null;
    public Encoding MessageEncoding
    {
        get
        {
            _msgEncoding ??= Encoding.UTF8;
            return _msgEncoding;
        }
        set => _msgEncoding = value;
    }

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

    public ClientWebSocket? NativeClient => GetSpecificOrThrow(_client);


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
        if (_disposing)
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
            _client!,
            status,
            statusDescription,
            false,
            false, null).ConfigureAwait(false);
        _disconnectedSubject.OnNext(DisconnectionInfo.Create(DisconnectionType.ByUser, _client!, null));
        return result;
    }

    public async Task<bool> StopOrFail(WebSocketCloseStatus status, string statusDescription)
    {
        var result = await StopInternal(
            _client!,
            status,
            statusDescription,
            true,
            false, null).ConfigureAwait(false);
        _disconnectedSubject.OnNext(DisconnectionInfo.Create(DisconnectionType.ByUser, _client!, null));
        return result;
    }

    private async Task<bool> StopInternal(WebSocket client, WebSocketCloseStatus status, string statusDescription,
            bool failFast, bool byServer, CancellationToken? cancellation)
    {
        if (_disposing)
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
            _logger.LogDebug("Client is already stopped");
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
    {
        Exception? causedException = null;
        try
        {
            const int chunkSize = 1024 * 4;
            var buffer = new ArraySegment<byte>(new byte[chunkSize]);
            do
            {
                WebSocketReceiveResult result;
                byte[]? resultArrayWithTrailing = null;
                var resultArraySize = 0;
                var isResultArrayCloned = false;
                MemoryStream? ms = null;
                while (true)
                {
                    result = await client.ReceiveAsync(buffer, token);
                    var currentChunk = buffer.Array;
                    var currentChunkSize = result.Count;
                    var isFirstChunk = resultArrayWithTrailing == null;
                    if (isFirstChunk)
                    {
                        // first chunk, use buffer as reference, do not allocate anything
                        resultArraySize += currentChunkSize;
                        resultArrayWithTrailing = currentChunk!;
                        isResultArrayCloned = false;
                    }
                    else if (currentChunk == null)
                    {
                        // weird chunk, do nothing
                    }
                    else
                    {
                        // received more chunks, lets merge them via memory stream
                        if (ms == null)
                        {
                            // create memory stream and insert first chunk
                            ms = new MemoryStream();
                            ms.Write(resultArrayWithTrailing!, 0, resultArraySize);
                        }

                        // insert current chunk
                        ms.Write(currentChunk, buffer.Offset, currentChunkSize);
                    }
                    if (result.EndOfMessage)
                        break;

                    if (isResultArrayCloned)
                        continue;

                    // we got more chunks incoming, need to clone first chunk
                    resultArrayWithTrailing = resultArrayWithTrailing?.ToArray();
                    isResultArrayCloned = true;
                }
                ms?.Seek(0, SeekOrigin.Begin);

                SocketResponse message;
                if (result.MessageType == WebSocketMessageType.Text && IsTextMessageConversionEnabled)
                {
                    var data = ms != null ?
                        MessageEncoding.GetString(ms.ToArray()) :
                        resultArrayWithTrailing != null ?
                            MessageEncoding.GetString(resultArrayWithTrailing, 0, resultArraySize) :
                            null;

                    message = SocketResponse.TextMessage(data);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogTrace("Received close message");

                    if (!IsStarted || _stopping)
                    {
                        return;
                    }

                    var info = DisconnectionInfo.Create(DisconnectionType.ByServer, client, null);
                    _disconnectedSubject.OnNext(info);

                    if (info.CancelClosing)
                    {
                        // closing canceled, reconnect if enabled
                        if (IsReconnectionEnabled)
                        {
                            throw new OperationCanceledException("Websocket connection was closed by server");
                        }

                        continue;
                    }

                    await StopInternal(client, WebSocketCloseStatus.NormalClosure, "Closing", false, true, token);

                    // reconnect if enabled
                    if (IsReconnectionEnabled && !ShouldIgnoreReconnection(client))
                    {
                        _ = ReconnectSynchronized(ReconnectionType.Lost, false, null);
                    }

                    return;
                }
                else
                {
                    if (ms != null)
                    {
                        message = SocketResponse.BinaryMessage(ms.ToArray());
                    }
                    else
                    {
                        Array.Resize(ref resultArrayWithTrailing, resultArraySize);
                        message = SocketResponse.BinaryMessage(resultArrayWithTrailing);
                    }
                }

                ms?.Dispose();

                _logger.LogTrace("Received:  [{message}]", message);
                _lastReceivedMsg = DateTime.UtcNow;
                _messageReceivedSubject.OnNext(message);

            } while (client.State == WebSocketState.Open && !token.IsCancellationRequested);

        }
        catch (TaskCanceledException e)
        {
            // task was canceled, ignore
            causedException = e;
        }
        catch (OperationCanceledException e)
        {
            // operation was canceled, ignore
            causedException = e;
        }
        catch (ObjectDisposedException e)
        {
            // client was disposed, ignore
            causedException = e;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while listening to websocket stream, ");
            causedException = e;
        }
        if (ShouldIgnoreReconnection(client) || !IsStarted)
            return;
        _ = ReconnectSynchronized(ReconnectionType.Lost, false, causedException);
    }



    private bool IsClientConnected() => _client?.State == WebSocketState.Open;
    private bool ShouldIgnoreReconnection(WebSocket client)
    {
        var inProgress = _disposing || _reconnecting || _stopping;
        var differentClient = client != _client;
        return inProgress || differentClient;
    }
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
    private static ClientWebSocket? GetSpecificOrThrow(WebSocket? client)
    {
        if (client == null)
            return null;
        if (client is not ClientWebSocket specific)
            throw new WebSocketException("Cannot cast 'WebSocket' client to 'ClientWebSocket', " +
                                         "provide correct type via factory or don't use this property at all.");
        return specific;
    }



    #region IDisposable
    private bool _disposing;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposing)
        {
            _disposing = true;
            if (disposing)
            {
                _logger.LogDebug("Disposing Client");
                try
                {
                    _messagesTextToSendQueue?.Writer.TryComplete();
                    _messagesBinaryToSendQueue?.Writer.TryComplete();
                    _cancellation?.Cancel();
                    _cancellationTotal?.Cancel();
                    _client?.Abort();
                    _client?.Dispose();
                    _lastChanceTimer?.Dispose();
                    _cancellation?.Dispose();
                    _cancellationTotal?.Dispose();
                    _messageReceivedSubject.OnCompleted();
                    _reconnectionSubject.OnCompleted();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to dispose Client");
                }
                if (IsRunning)
                    _disconnectedSubject.OnNext(DisconnectionInfo.Create(DisconnectionType.Exit, _client!, null));
                IsRunning = false;
                IsStarted = false;
                _disconnectedSubject.OnCompleted();
            }
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
