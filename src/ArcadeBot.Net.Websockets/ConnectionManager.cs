using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ArcadeBot.Core;
using ArcadeBot.Net.Websockets.Models;
using ArcadeBot.Net.Websockets.Models.Gateway.Events;
using ArcadeBot.Net.WebSockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArcadeBot.Net.Websockets;

public class ConnectionManager : IDisposable
{
    private readonly ILogger<ConnectionManager> _logger;
    private readonly DiscordWebsocketClient _discordGatewayClient;
    private readonly BotOptions _config;
    private readonly Subject<DiscordGatewayMessage?> _gatewayEvent = new();
    private CancellationTokenSource? _heartbeatToken;
    private bool disposedValue;

    public IObservable<DiscordGatewayMessage?> GatewayEvent => _gatewayEvent.AsObservable();

    public ConnectionManager(ILogger<ConnectionManager> logger, IOptions<BotOptions> botConfig, DiscordWebsocketClient discordGatewayClient)
    {
        _logger = logger;
        _discordGatewayClient = discordGatewayClient;
        _config = botConfig.Value;
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _discordGatewayClient.ReconnectionHappened.Subscribe(info =>
        {
            if (info.Type is ReconnectionType.Initial)
                return;
            _heartbeatToken?.Cancel();
            _heartbeatToken = new CancellationTokenSource();
        });
        _discordGatewayClient
            .MessageReceived
            .Select(ParseMessage).Subscribe(async gatewayEvent => _gatewayEvent.OnNext(await gatewayEvent));
        GatewayEvent.Where(x => x?.OpCode is DTO.OpCodes.Gateway.Hello).Subscribe(async msg =>
        {
            await SendIdentify();
            HandleHeartBeat(msg?.EventData.Deserialize<HelloEvent>());
        });
        GatewayEvent.Where(x => x?.OpCode is not DTO.OpCodes.Gateway.Hello).Subscribe(msg =>
                        _logger.LogTrace("Received: [{msg}]", msg));
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        _heartbeatToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

        await _discordGatewayClient.Start();

        await Task.Delay(-1, stoppingToken);
    }

    private async Task<DiscordGatewayMessage?> ParseMessage(SocketResponse socketMsg)
    {
        if (socketMsg.MessageType is WebSocketMessageType.Text)
            return JsonSerializer.Deserialize<DiscordGatewayMessage>(socketMsg.Text!);

        if (socketMsg.MessageType is WebSocketMessageType.Binary)
        {
            var str = await DecodeBinMessage(socketMsg.Binary!);
            return JsonSerializer.Deserialize<DiscordGatewayMessage>(str);
        }
        throw new InvalidOperationException("This type of message is not supported");
    }

    private static async Task<string> DecodeBinMessage(byte[] binaryData)
    {
        if (binaryData == null)
            throw new InvalidOperationException("Data cant be null");
        using var compressed = new MemoryStream(binaryData);
        if (compressed.ReadByte() != 0x78 || compressed.ReadByte() != 0x9C)//zlib header
            throw new InvalidOperationException("Incorrect zlib header");
        using var deflate = new DeflateStream(compressed, CompressionMode.Decompress);

        using var sr = new StreamReader(deflate);
        return await sr.ReadToEndAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _heartbeatToken?.Cancel();
                _heartbeatToken?.Dispose();
                _discordGatewayClient.Dispose();
                _gatewayEvent.OnCompleted();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }



    private Task SendIdentify()
    {
        var intents = (GatewayIntents.AllUnprivileged & ~(GatewayIntents.GuildScheduledEvents | GatewayIntents.GuildInvites)) |
            GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.MessageContent;
        var message = new DiscordGatewayMessage
        {
            OpCode = DTO.OpCodes.Gateway.Identify,
            EventData = JsonValue.Create(new IdentifyEvent(
                _config.Token,
                new Dictionary<string, string>
                {
                    ["os"] = "Windwos",
                    ["browser"] = ".NET",
                    ["device"] = ".NET"
                },
                intents,
                true, 50, new[] { 0, 1 },
                new PresenceUpdateParams("dnd", 0, false, new List<Activity> { new Activity(0, 0, "Being reworked from scratch", "Being reworked from scratch") })
            ))
        };
        return _discordGatewayClient.SendInstant(message);
    }

    private void HandleHeartBeat(HelloEvent? args)
    {
        ArgumentNullException.ThrowIfNull(args);
        _ = Task.Factory.StartNew(_ => SendHeartBeat(args.Interval), TaskCreationOptions.LongRunning, _heartbeatToken!.Token);
    }

    private async Task SendHeartBeat(int delay)
    {
        try
        {
            _logger.LogDebug("Heartbeat Started");
            while (!_heartbeatToken!.Token.IsCancellationRequested)
            {
                await _discordGatewayClient.SendInstant("{ \"op\": 1, \"d\": 0 }");
                await Task.Delay(delay, _heartbeatToken!.Token);
            }
        }
        catch (TaskCanceledException) { }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            _logger.LogTrace(e, "Sending heartbeat thread failed, Creating new thread");
            _ = Task.Factory
                .StartNew(_ => SendHeartBeat(delay), TaskCreationOptions.LongRunning, _heartbeatToken!.Token);
        }
    }
}
