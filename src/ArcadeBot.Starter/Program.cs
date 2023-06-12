using ArcadeBot.Net.WebSockets;
using Serilog;

namespace ArcadeBot.Starter;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.FromLogContext()
        .WriteTo.File("Debug.log", Serilog.Events.LogEventLevel.Debug, 
        rollingInterval: RollingInterval.Hour,
        outputTemplate: "[{Timestamp:HH:mm:ss} {SourceContext:l} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {SourceContext:l} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .CreateLogger();
        IHost host = Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureServices((ctx, services) =>
            {
                services.AddDiscordWebsockets(ctx.Configuration.GetSection("Bot"));
                services.AddHostedService<Worker>();
            })
            .Build();

        await host.RunAsync();

        Console.ReadKey();
    }
}