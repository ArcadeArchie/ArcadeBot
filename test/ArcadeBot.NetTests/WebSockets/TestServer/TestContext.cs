using ArcadeBot.Net.WebSockets;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Extensions.Logging;

namespace ArcadeBot.NetTests.WebSockets.TestServer;

public class TestContext<TStartup> where TStartup : class
{
    private readonly TestServerApplicationFactory<TStartup> _factory;

    public WebSocketClient NativeTestClient { get; set; } = null!;

    public Uri InvalidUri { get; } = new("wss://invalid-url.local");

    internal Microsoft.Extensions.Logging.ILogger<DiscordWebsocketClient> Logger { get; } = 
        new SerilogLoggerFactory(Log.Logger).CreateLogger<DiscordWebsocketClient>();
    
    public TestContext(ITestOutputHelper output)
    {
        _factory = new TestServerApplicationFactory<TStartup>();
        InitLogging(output);
    }
    internal DiscordWebsocketClient CreateClient()
    {
        _ = _factory.CreateClient(); // This is needed since _factory.Server would otherwise be null
        return CreateClient(_factory.Server.BaseAddress);
    }

    internal DiscordWebsocketClient CreateClient(Uri serverUrl)
    {
        var wsUri = new UriBuilder(serverUrl)
        {
            Scheme = "ws",
            Path = "ws"
        }.Uri;

        return new DiscordWebsocketClient(Logger, wsUri,
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

    internal DiscordWebsocketClient CreateInvalidClient(Uri serverUrl)
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
            .MinimumLevel.Debug()
            .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Verbose)
            .CreateLogger();
    }
}