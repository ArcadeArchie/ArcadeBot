using ArcadeBot.Core;
using Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArcadeBot.Net.WebSockets
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddDiscordWebsockets(this IServiceCollection services, IConfiguration botOptions)
        {
            services
                .AddMediator()
                .Configure<BotOptions>(botOptions)
                .AddSingleton(new Uri(Constants.WSSUrl + "/?v=10&encoding=json"))
                .AddScoped<DiscordWebsocketClient>()
                .AddScoped(svc => new ConnectionManager(
                    svc.GetRequiredService<ILogger<ConnectionManager>>(),
                    svc.GetRequiredService<IOptions<BotOptions>>(),
                    svc.GetRequiredService<DiscordWebsocketClient>()
                ));
            return services;
        }
    }
}