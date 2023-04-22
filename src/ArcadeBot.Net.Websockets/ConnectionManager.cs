using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ArcadeBot.Converters;
using ArcadeBot.Core;
using ArcadeBot.DTO;
using ArcadeBot.DTO.Gateway;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArcadeBot.Net.WebSockets
{
    public class ConnectionManager : IDisposable
    {
        public int Latency { get; protected set; }
        public CancellationToken CancelToken { get; private set; }

        private readonly ILogger<ConnectionManager> _logger;
        private readonly DiscordWebsocketClient _clientSocket;
        private readonly IMediator _mediator;
        private readonly ConcurrentQueue<long> _heartbeatTimes;
        private readonly BotOptions _botConfig;
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new GuildFeaturesJsonConverter() }
        };
        private Task? _heartbeatTask;
        private int _lastSeq;
        private long _lastMessageTime;

        internal ConnectionManager(ILogger<ConnectionManager> logger, DiscordWebsocketClient clientSocket, IMediator mediator, IOptions<BotOptions> botConfig)
        {
            _logger = logger;
            _clientSocket = clientSocket;
            _mediator = mediator;
            _botConfig = botConfig.Value;
            if (string.IsNullOrEmpty(_botConfig.Token))
                throw new InvalidOperationException("Invalid bot token");
            _heartbeatTimes = new ConcurrentQueue<long>();
            _clientSocket.ReceivedGatewayEvent += HandleGatewayEvent;
        }

        public async Task ConnectAsync(CancellationToken stoppingToken)
        {
            CancelToken = stoppingToken;
            _logger.LogInformation("Connecting");
            await _clientSocket.ConnectAsync();
            _logger.LogInformation("Connected");
            await IdentifyAsync();
        }

        public async Task DisconnectAsync()
        {
            _logger.LogInformation("Disconnecting");
            await _clientSocket.DisconnectAsync();
            _logger.LogInformation("Disconnected");
        }



        private async Task? StartHeartbeatAsync(HelloEvent? helloEventArgs, CancellationToken token)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(helloEventArgs);
                while (!token.IsCancellationRequested)
                {
                    int now = Environment.TickCount;
                    if (!_heartbeatTimes.IsEmpty && (now - _lastMessageTime) > helloEventArgs!.Interval)
                    {
                        //TODO: handle missed heartbeat 
                        _logger.LogCritical("Server missed last heartbeat");
                        throw new Exception();
                    }
                    _heartbeatTimes.Enqueue(now);
                    /*if (!*/
                    await HeartbeatAsync(_lastSeq);
                        // throw new SocketException();
                    await Task.Delay(helloEventArgs!.Interval, token);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<bool> HeartbeatAsync(int lastSeq)
        {
            var message = new SocketFrame
            {
                OpCode = OpCodes.Gateway.Heartbeat,
                EventData = lastSeq
            };
            return await _clientSocket.SendAsync(message);
        }

        private async Task IdentifyAsync()
        {
            var identify = new Identify
            {
                Token = _botConfig.Token,
                LargeThreshold = 100,
                Intents = (int)GatewayIntents.AllUnprivileged,
                Presence = new PresenceUpdateParams
                {
                    Status = "dnd", //https://discord.com/developers/docs/topics/gateway-events#update-presence-status-types
                    IsAFK = false,
                    IdleSince = 0,
                    Activities = new List<Activity> { new Activity { Type = 0, Name = "Being reworked from scratch", Details = "Being reworked from scratch", Flags = 0 } }
                },
                Properties = new Dictionary<string, string>
                {
                    {"device", "CSharp"},
                    {"os", Environment.OSVersion.Platform.ToString() },
                    {"browser", "CSharp"},
                },
                ShardingParams = new[] { 0, 1 }
            };
            var message = new SocketFrame
            {
                OpCode = OpCodes.Gateway.Identify,
                EventData = JsonValue.Create(identify)
            };
            await _clientSocket.SendAsync(message);
        }


        #region EventHandlers


        private Task HandleGatewayEvent(SocketFrame message)
        {
            if (message!.Sequence != null)
                _lastSeq = message.Sequence.Value;
            _lastMessageTime = Environment.TickCount;
            if (_heartbeatTask != null && message.OpCode == OpCodes.Gateway.Hello)// hotfix send re-identify
                return IdentifyAsync();
            if (message.OpCode == OpCodes.Gateway.Hello || message.OpCode == OpCodes.Gateway.HeartbeatACK)
            {
                return HeartBeat();
            }

            if (message.OpCode == OpCodes.Gateway.Dispatch)
                return Dispatch();

            return Task.CompletedTask;

            Task Dispatch()
            {
                if (message.EventData != null)
                {
                    object? eventData = message.EventName switch
                    {
                        // "READY" => message.EventData.ToJsonString(),
                        "GUILD_CREATE" => message.EventData.Deserialize<ExtendedGuild>(_serializerOptions), // do deserialization to proper DTOs
                        _ => message.EventData.ToJsonString()
                    };
                    _logger.LogDebug("Dispatch recieved, eventName: [{name}], eventData: [{data}]", message.EventName, eventData);
                }
                return Task.CompletedTask;
            }


            Task HeartBeat()
            {
                switch (message!.OpCode)
                {
                    case OpCodes.Gateway.Hello:
                        HandleHeartbeat(message?.EventData?.Deserialize<HelloEvent>());
                        break;
                    case OpCodes.Gateway.HeartbeatACK:
                        HandleHeartbeat();
                        break;
                }
                return Task.CompletedTask;
            }
        }


        private void HandleHeartbeat(HelloEvent? eventArgs = null)
        {
            if (eventArgs is not null)
            {
                _logger.LogDebug("Recieved Hello, starting heartbeat");
                _heartbeatTask ??= StartHeartbeatAsync(eventArgs, CancelToken);
                return;
            }
            if (!_heartbeatTimes.TryDequeue(out long time))
                return;
            int latency = (int)(Environment.TickCount - time);
            int before = Latency;
            Latency = latency;
            _logger.LogDebug("[HeartbeatACK]: Latency {latency}, old {oldLatency}", latency, before);
        }
        #endregion


        #region IDisposable
        private bool isDisposed;
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
                return;
            if (disposing)
            {
                _clientSocket.Dispose();
                _heartbeatTask?.Dispose();
            }
            isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~ConnectionManager()
        {
            Dispose(false);
        }
        #endregion
    }
}