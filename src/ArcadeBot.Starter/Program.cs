using ArcadeBot.Core;
using ArcadeBot.Net.WebSockets;
using Serilog;

namespace ArcadeBot.Starter;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {SourceContext:l} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .CreateLogger();
        IHost host = Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureServices((ctx, services) =>
            {
                services.Configure<BotOptions>(ctx.Configuration.GetSection("Bot"));
                services
                .AddMediator()
                .AddDiscordWebsockets()
                .AddHostedService<WebSocketWorker>();
                // services.AddHostedService<WebSocketWorker>();
            })
            .Build();

        await host.RunAsync();

        Console.ReadKey();
    }
}