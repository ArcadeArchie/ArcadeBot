using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace ArcadeBot.Core.Interactions;

public class InteractionHandler
{
    private readonly ILogger<InteractionHandler> _logger;
    private readonly IServiceProvider _services;
    private readonly InteractionService _slashCommands;
    private readonly DiscordSocketClient _client;



    public InteractionHandler(ILogger<InteractionHandler> logger, IServiceProvider services, InteractionService slashCommands, DiscordSocketClient client)
    {
        _logger = logger;
        _services = services;
        _slashCommands = slashCommands;
        _client = client;

        _client.InteractionCreated += HandleInteraction;
    }

    internal async Task InitAsync(CancellationToken stoppingToken)
    {
        await _slashCommands.AddModulesAsync(Assembly.GetAssembly(typeof(DiscordBot)), _services);
    }


    

    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            var ctx = new SocketInteractionContext(_client, arg);
            var result = await _slashCommands.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occoured while handling a interaction");
        }
    }
}
