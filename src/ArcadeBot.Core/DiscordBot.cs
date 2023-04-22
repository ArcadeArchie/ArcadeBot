using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArcadeBot.Core.Interactions;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArcadeBot.Core;

public class DiscordBot
{
    private readonly ILogger<DiscordBot> _logger;
    private readonly DiscordSocketClient _client;
    private readonly BotOptions _config;
    private readonly InteractionHandler _interactionHandler;

    public DiscordBot(ILogger<DiscordBot> logger, DiscordSocketClient client, IOptions<BotOptions> botConfig, InteractionHandler interactionHandler)
    {
        _logger = logger;
        _client = client;
        _config = botConfig.Value;
        _interactionHandler = interactionHandler;

        _client.Ready += OnReadyAsync;
        _client.Log += OnLogAsync;
    }


    public async Task ConnectAsync(CancellationToken stoppingToken)
    {
        await _client.LoginAsync(TokenType.Bot, _config.Token);
        await _client.StartAsync();
        await _interactionHandler.InitAsync(stoppingToken);
    }

    public async Task DisconnectAsync()
    {
        await _client.StopAsync();
    }

    private Task OnReadyAsync()
    {
        _logger.LogInformation("Bot is ready");
        return Task.CompletedTask;
    }
    private Task OnLogAsync(LogMessage msg)
    {
        string logText = $"{msg.Exception?.ToString() ?? msg.Message}";
        switch (msg.Severity)
        {
            case LogSeverity.Critical:
                {
                    _logger.LogCritical(msg.Exception, logText);
                    break;
                }
            case LogSeverity.Warning:
                {
                    _logger.LogWarning(logText);
                    break;
                }
            case LogSeverity.Info:
                {
                    _logger.LogInformation(logText);
                    break;
                }
            case LogSeverity.Verbose:
                {
                    _logger.LogInformation(msg.Exception, logText);
                    break;
                }
            case LogSeverity.Debug:
                {
                    _logger.LogDebug(msg.Exception, logText);
                    break;
                }
            case LogSeverity.Error:
                {
                    _logger.LogError(msg.Exception, logText);
                    break;
                }
        }
        return Task.CompletedTask;

    }
}
