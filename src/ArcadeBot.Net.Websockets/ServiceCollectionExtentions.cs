using ArcadeBot.Core;
using Microsoft.Extensions.DependencyInjection;

namespace ArcadeBot.Net.WebSockets
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddDiscordWebsockets(this IServiceCollection services) => services
                .AddSingleton(new Uri(Constants.WSSUrl + "/?v=10&encoding=json"))
                .AddScoped<DiscordWebsocketClient>()
                .AddScoped<ConnectionManager>();
    }
}