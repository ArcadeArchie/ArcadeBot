using ArcadeBot.Core;
using ArcadeBot.Net.Websockets;
using Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArcadeBot.Net.Websockets
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
                .AddScoped<ConnectionManager>();
            return services;
        }
    }
}