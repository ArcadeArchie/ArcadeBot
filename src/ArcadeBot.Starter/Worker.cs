using ArcadeBot.Net.WebSockets;

namespace ArcadeBot.Starter;

public sealed class Worker : BackgroundService
{
    private readonly ConnectionManager _client;
    private readonly IServiceScope _workerScope;

    public Worker(IServiceProvider services)
    {
        _workerScope = services.CreateScope();
        _client = _workerScope.ServiceProvider.GetRequiredService<ConnectionManager>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _client.StartAsync(stoppingToken);
    }

    public override void Dispose()
    {
        base.Dispose();
        _client.Dispose();
        _workerScope.Dispose();
    }
}
