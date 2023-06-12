using ArcadeBot.Net.WebSockets.Models;
using Microsoft.Extensions.Logging;

namespace ArcadeBot.Net.WebSockets;

///Reconnect Logic for <see cref="DiscordWebsocketClient"/>
internal partial class DiscordWebsocketClient
{

    /// <summary>
    /// Force reconnection. 
    /// Closes current websocket stream and perform a new connection to the server.
    /// </summary>
    /// <remarks>In case of connection error it doesn't throw an exception, but tries to reconnect indefinitely. </remarks>
    public Task Reconnect() => ReconnectInternal(false);

    /// <summary>
    /// Force reconnection. 
    /// Closes current websocket stream and perform a new connection to the server.
    /// </summary>
    /// <remarks>In case of connection error it throws an exception and doesn't perform any other reconnection try. </remarks>
    public Task ReconnectOrFail() => ReconnectInternal(true);

    /// <summary>
    /// Closes current websocket stream and perform a new connection to the server.
    /// </summary>
    /// <param name="fastFail">Throw exception if connection fails</param>
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

    /// <summary>
    /// Disable missed message detection
    /// </summary>
    private void DeactivateLastChance()
    {
        _lastChanceTimer?.Dispose();
        _lastChanceTimer = null;
    }
    /// <summary>
    /// Enable missed message detection
    /// </summary>
    private void ActivateLastChance()
    {
        var timerMs = 1000 * 1;
        _lastChanceTimer = new Timer(LastChance, null, timerMs, timerMs);
    }
    /// <summary>
    /// Hard restart the connection if remote hasnt sent a message in <see cref="ReconnectTimeout"/> 
    /// </summary>
    /// <param name="state"></param>
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

    /// <summary>
    /// Reconnect the websocket to the remote end, asynchronously locking it for thread safety
    /// </summary>
    /// <param name="type">Reconnect reason</param>
    /// <param name="failFast">Throw exception if connecting fails</param>
    /// <param name="causedException">Exception that caused the reconnect</param>
    private async Task ReconnectSynchronized(ReconnectionType type, bool failFast, Exception? causedException)
    {
        using (await _lock.LockAsync())
        {
            await Reconnect(type, failFast, causedException);
        }
    }
    /// <summary>
    /// Reconnect the websocket to the remote end
    /// </summary>
    /// <param name="type">Reconnect reason</param>
    /// <param name="failFast">Throw exception if connecting fails</param>
    /// <param name="causedException">Exception that caused the reconnect</param>
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
