using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using ArcadeBot.Core;
using ArcadeBot.Net.Websockets.API.Gateway;
using ArcadeBot.Net.WebSockets.API;
using ArcadeBot.Net.WebSockets.API.Gateway;
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
        private readonly IOptions<BotOptions> _botConfig;

        private Task? _heartbeatTask;
        private int _lastSeq;
        private long _lastMessageTime;

        internal ConnectionManager(ILogger<ConnectionManager> logger, DiscordWebsocketClient clientSocket, IMediator mediator, IOptions<BotOptions> botConfig)
        {
            _logger = logger;
            _clientSocket = clientSocket;
            _mediator = mediator;

            _heartbeatTimes = new ConcurrentQueue<long>();
            _clientSocket.ReceivedGatewayEvent += HandleHeartbeat;
            _botConfig = botConfig;
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

        public void Dispose()
        {
            _clientSocket.Dispose();
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
                    }
                    _heartbeatTimes.Enqueue(now);
                    await HeartbeatAsync(_lastSeq);
                    await Task.Delay(helloEventArgs!.Interval, token);
                }
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        private async Task HeartbeatAsync(int lastSeq)
        {
            var message = new SocketFrame
            {
                OpCode = OpCodes.Gateway.Heartbeat,
                EventData = lastSeq
            };
            await _clientSocket.SendAsync(message);
        }

        private async Task IdentifyAsync()
        {
            var identify = new Identify
            {
                Token = _botConfig.Value.Token,
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


        private Task HandleHeartbeat(SocketFrame message)
        {
            if (message!.Sequence != null)
                _lastSeq = message.Sequence.Value;
            _lastMessageTime = Environment.TickCount;
            
            if (message.OpCode != OpCodes.Gateway.Hello && message.OpCode != OpCodes.Gateway.HeartbeatACK)
                return Task.CompletedTask;
            switch (message!.OpCode)
            {
                case OpCodes.Gateway.Hello:
                    _logger.LogDebug("Recieved Hello, starting heartbeat");
                    _heartbeatTask ??= StartHeartbeatAsync(message?.EventData?.Deserialize<HelloEvent>(), CancelToken);
                    break;
                case OpCodes.Gateway.HeartbeatACK:
                    if (!_heartbeatTimes.TryDequeue(out long time))
                        break;
                    int latency = (int)(Environment.TickCount - time);
                    int before = Latency;
                    Latency = latency;
                    _logger.LogDebug("[HeartbeatACK]: Latency {latency}, old {oldLatency}", latency, before);
                    break;
            }
            return Task.CompletedTask;
        }

        #endregion
    }
}