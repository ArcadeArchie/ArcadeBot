using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcadeBot.Core.Interactions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArcadeBot.Core;

public static class IServiceCollectionExtentions
{
    private const GatewayIntents DefaultIntents =
            (GatewayIntents.AllUnprivileged & ~(GatewayIntents.GuildScheduledEvents | GatewayIntents.GuildInvites)) |
            GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.MessageContent;

    public static IServiceCollection AddDiscordBot(this IServiceCollection services, IConfigurationSection botConfig)
    {

        return
        services
        .AddMediator()
        .Configure<BotOptions>(botConfig)
        .AddSingleton<DiscordSocketClient>(svc => new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = DefaultIntents,
            AlwaysDownloadUsers = true,
            LogLevel = LogSeverity.Debug
        }))
        .AddSingleton<InteractionService>(svc => new InteractionService(svc.GetRequiredService<DiscordSocketClient>()))
        .AddSingleton<InteractionHandler>()
        .AddSingleton<DiscordBot>();
    }
}
