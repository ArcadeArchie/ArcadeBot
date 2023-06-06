using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading.Tasks;
using ArcadeBot.Net.Websockets.Models;
using ArcadeBot.Net.WebSockets;
using Microsoft.Extensions.Logging;

namespace ArcadeBot.Net.Websockets;

public class ConnectionManager : IDisposable
{
    private readonly ILogger<ConnectionManager> _logger;
    private readonly Subject<DiscordGatewayMessage?> _gatewayEvent = new();
    private readonly DiscordWebsocketClient _discordGatewayClient;
    private CancellationTokenSource? _heartbeatToken;
    private bool disposedValue;

    public IObservable<DiscordGatewayMessage?> GatewayEvent => _gatewayEvent.AsObservable();

    public ConnectionManager(DiscordWebsocketClient discordGatewayClient, ILogger<ConnectionManager> logger)
    {
        _logger = logger;
        _discordGatewayClient = discordGatewayClient;
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
            HandleHeartBeat(msg!);
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
        var intents = (int)((GatewayIntents.AllUnprivileged & ~(GatewayIntents.GuildScheduledEvents | GatewayIntents.GuildInvites)) |
            GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.MessageContent);
        return _discordGatewayClient.SendInstant("{ \"op\": 2, \"d\": { \"token\": \"\", \"properties\": { \"os\": \"linux\", \"browser\": \"disco\", \"device\": \"disco\" }, \"compress\": true, \"large_threshold\": 250, \"shard\": [0, 1], \"presence\": { \"activities\": [{ \"name\": \"Being reworked from scratch\", \"type\": 0 }], \"status\": \"dnd\", \"since\": 0, \"afk\": false }, \"intents\": " + intents + " } }");
    }

    private void HandleHeartBeat(DiscordGatewayMessage msg)
    {
        _ = Task.Factory.StartNew(_ => SendHeartBeat(msg.EventData!["heartbeat_interval"]!.GetValue<int>()), TaskCreationOptions.LongRunning, _heartbeatToken!.Token);
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
