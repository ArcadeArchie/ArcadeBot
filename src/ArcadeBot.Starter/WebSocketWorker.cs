using ArcadeBot.Net.WebSockets;

namespace ArcadeBot.Starter
{
    internal class WebSocketWorker : BackgroundService
    {
        private readonly IServiceScope _workerScope;
        private readonly ConnectionManager _connection;
        public WebSocketWorker(ILogger<WebSocketWorker> logger, IServiceProvider workerScope)
        {
            _workerScope = workerScope.CreateScope();
            _connection = _workerScope.ServiceProvider.GetRequiredService<ConnectionManager>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _connection.ConnectAsync(stoppingToken);
            await Task.Delay(-0, stoppingToken); // run until stopped
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _connection.DisconnectAsync();
            await base.StopAsync(cancellationToken);
        }
        
        public override void Dispose()
        {
            base.Dispose();
            _connection.Dispose();
            _workerScope.Dispose();
        }
    }
}