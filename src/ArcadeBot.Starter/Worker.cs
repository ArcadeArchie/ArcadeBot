using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text.Json;
using ArcadeBot.Net.Websockets;
using ArcadeBot.Net.Websockets.Models;
using ArcadeBot.Net.WebSockets;
using Microsoft.AspNetCore.Components.Forms;

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
