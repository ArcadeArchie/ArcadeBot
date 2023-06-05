using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcadeBot.Net.Websockets;
using Microsoft.AspNetCore.TestHost;
using Moq;
using Serilog;
using Xunit.Abstractions;

namespace ArcadeBot.NetTests.WebSockets.TestServer;

public class TestContext<TStartup> where TStartup : class
{
    private readonly TestServerApplicationFactory<TStartup> _factory;

    public WebSocketClient NativeTestClient { get; set; } = null!;

    public Uri InvalidUri { get; } = new("wss://invalid-url.local");

    public TestContext(ITestOutputHelper output)
    {
        _factory = new TestServerApplicationFactory<TStartup>();
        InitLogging(output);
    }
    public DiscordWebsocketClient CreateClient()
    {
        var httpClient = _factory.CreateClient(); // This is needed since _factory.Server would otherwise be null
        return CreateClient(_factory.Server.BaseAddress);
    }

    public DiscordWebsocketClient CreateClient(Uri serverUrl)
    {
        var wsUri = new UriBuilder(serverUrl)
        {
            Scheme = "ws",
            Path = "ws"
        }.Uri;

        var mock = new Mock<Microsoft.Extensions.Logging.ILogger<DiscordWebsocketClient>>();
        return new DiscordWebsocketClient(mock.Object, wsUri,
            async (uri, token) =>
            {
                if (_factory.Server == null)
                {
                    throw new InvalidOperationException("Connection to websocket server failed, check url");
                }

                if (uri == InvalidUri)
                {
                    throw new InvalidOperationException("Connection to websocket server failed, check url");
                }

                NativeTestClient = _factory.Server.CreateWebSocketClient();
                var ws = await NativeTestClient.ConnectAsync(uri, token).ConfigureAwait(false);
                //await Task.Delay(1000, token);
                return ws;
            });
    }

    public DiscordWebsocketClient CreateInvalidClient(Uri serverUrl)
    {
        var wsUri = new UriBuilder(serverUrl)
        {
            Scheme = "ws",
            Path = "ws"
        }.Uri;
        var mock = new Mock<Microsoft.Extensions.Logging.ILogger<DiscordWebsocketClient>>();
        return new DiscordWebsocketClient(mock.Object, wsUri,
            (uri, token) => throw new InvalidOperationException("Connection to websocket server failed, check url"));
    }
    private void InitLogging(ITestOutputHelper output)
    {
        if (output == null)
            return;
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Verbose)
            .CreateLogger();
    }
}
