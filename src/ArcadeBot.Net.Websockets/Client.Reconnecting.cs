using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcadeBot.Net.Websockets.Models;
using Microsoft.Extensions.Logging;

namespace ArcadeBot.Net.Websockets;

internal partial class DiscordWebsocketClient
{

    public Task Reconnect() => ReconnectInternal(false);

    public Task ReconnectOrFail() => ReconnectInternal(true);

    private async Task ReconnectInternal(bool fastFail)
    {
        if (!IsStarted)
        {
            _logger.LogDebug("Client not started, ignoring reconnection..");
            return;
        }

        try
        {
            await ReconnectSynchronized(ReconnectionType.ByUser, fastFail, null).ConfigureAwait(false);
        }
        finally
        {
            _reconnecting = false;
        }
    }


    private void DeactivateLastChance()
    {
        _lastChanceTimer?.Dispose();
        _lastChanceTimer = null;
    }
    private void ActivateLastChance()
    {
        var timerMs = 1000 * 1;
        _lastChanceTimer = new Timer(LastChance, null, timerMs, timerMs);
    }
    private void LastChance(object? state)
    {
        if (!IsReconnectionEnabled || ReconnectTimeout == null)
        {
            // reconnection disabled, do nothing
            DeactivateLastChance();
            return;
        }

        var timeoutMs = Math.Abs(ReconnectTimeout.Value.TotalMilliseconds);
        var diffMs = Math.Abs(DateTime.UtcNow.Subtract(_lastReceivedMsg).TotalMilliseconds);
        if (diffMs > timeoutMs)
        {
            _logger.LogDebug("Last message received more than {timeoutMs:F} ms ago. Hard restart..", timeoutMs);

            DeactivateLastChance();
            _ = ReconnectSynchronized(ReconnectionType.NoMessageReceived, false, null);
        }
    }

    private async Task ReconnectSynchronized(ReconnectionType type, bool failFast, Exception? causedException)
    {
        using (await _lock.LockAsync())
        {
            await Reconnect(type, failFast, causedException);
        }
    }
    private async Task Reconnect(ReconnectionType type, bool failFast, Exception? causedException)
    {
        IsRunning = false;
        if (_disposing || !IsStarted)
        {
            // client already disposed or stopped manually
            return;
        }

        _reconnecting = true;

        var disType = TranslateTypeToDisconnection(type);
        var disInfo = DisconnectionInfo.Create(disType, _client!, causedException);
        if (type != ReconnectionType.Error)
        {
            _disconnectedSubject.OnNext(disInfo);
            if (disInfo.CancelReconnection)
            {
                // reconnection canceled by user, do nothing
                _logger.LogInformation("Reconnecting canceled by user, exiting.");
            }
        }

        _cancellation?.Cancel();
        try
        {
            _client?.Abort();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception while aborting client.");
        }
        _client?.Dispose();

        if (!IsReconnectionEnabled || disInfo.CancelReconnection)
        {
            // reconnection disabled, do nothing
            IsStarted = false;
            _reconnecting = false;
            return;
        }

        _logger.LogDebug("Reconnecting...");
        _cancellation = new CancellationTokenSource();
        await StartClient(Url, type, failFast, _cancellation.Token).ConfigureAwait(false);
        _reconnecting = false;
    }
}
