using ArcadeBot.Core;

namespace ArcadeBot.Starter
{
    internal class WebSocketWorker : BackgroundService
    {
        private readonly IServiceScope _workerScope;
        private readonly DiscordBot _bot;
        public WebSocketWorker(ILogger<WebSocketWorker> logger, IServiceProvider workerScope)
        {
            _workerScope = workerScope.CreateScope();
            _bot = _workerScope.ServiceProvider.GetRequiredService<DiscordBot>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _bot.ConnectAsync(stoppingToken);
            await Task.Delay(-1, stoppingToken); // run until stopped
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _bot.DisconnectAsync();
            await base.StopAsync(cancellationToken);
        }
        
        public override void Dispose()
        {
            base.Dispose();
            _workerScope.Dispose();
        }
    }
}