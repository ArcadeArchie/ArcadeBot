using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;

namespace ArcadeBot.Net.WebSockets;

/// Sending Logic for <see cref="DiscordWebsocketClient"/>
internal partial class DiscordWebsocketClient
{
    /// <summary>
    /// Text message queue
    /// </summary>
    private readonly Channel<string> _messagesTextToSendQueue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions()
    {
        SingleReader = true,
        SingleWriter = false
    });

    /// <summary>
    /// Binary message queue
    /// </summary>
    private readonly Channel<ArraySegment<byte>> _messagesBinaryToSendQueue = Channel.CreateUnbounded<ArraySegment<byte>>(new UnboundedChannelOptions()
    {
        SingleReader = true,
        SingleWriter = false
    });

    /// <summary>
    /// Post text message to queue to be sent
    /// </summary>
    /// <param name="message">UTF8 string text</param>
    /// <remarks>The sending work is done in a seperate thread</remarks>
    public void Send(string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        _messagesTextToSendQueue.Writer.TryWrite(message);
    }

    /// <summary>
    /// Post binary message to queue to be sent
    /// </summary>
    /// <param name="message">UTF8 binary message</param>
    /// <remarks>The sending work is done in a seperate thread</remarks>
    public void Send(byte[] message)
    {
        if (Equals(message, default(byte[])))
            throw new ArgumentNullException(nameof(message), "Message cant be null");
        _messagesBinaryToSendQueue.Writer.TryWrite(new ArraySegment<byte>(message));
    }

    /// <summary>
    /// Post binary message to queue to be sent
    /// </summary>
    /// <param name="message">UTF8 binary message</param>
    /// <remarks>The sending work is done in a seperate thread</remarks>
    public void Send(ArraySegment<byte> message)
    {
        if (Equals(message, default(ArraySegment<byte>)))
            throw new ArgumentNullException(nameof(message), "Message cant be null");
        _messagesBinaryToSendQueue.Writer.TryWrite(message);
    }

    /// <summary>
    /// Instantly send text message bypassing the send queue
    /// </summary>
    /// <param name="message">UTF8 text message</param>
    /// <remarks>Beware of the issue when sending two messages at once</remarks>
    public Task SendInstant(string message) => SendInternalSynchronized(message);

    /// <summary>
    /// Instantly send binary message bypassing the send queue
    /// </summary>
    /// <param name="message">UTF8 binary message</param>
    /// <remarks>Beware of the issue when sending two messages at once</remarks>
    public Task SendInstant(byte[] message) => SendInternalSynchronized(new ArraySegment<byte>(message));

    /// <summary>
    /// Stream/publish fake message (via 'MessageReceived' observable).
    /// Use for testing purposes to simulate a server message. 
    /// </summary>
    /// <param name="message">Message to be stream</param>
    /// <remarks>Even though its accepts a nullable, it will throw a <see cref="ArgumentNullException"/> if <paramref name="message"/> is null </remarks>
    public void StreamFakeMessage(SocketResponse? message)
    {
        ArgumentNullException.ThrowIfNull(message);
        _messageReceivedSubject.OnNext(message);
    }


    /// <summary>
    /// Read the text message queue and send messages to remote 
    /// </summary>
    private async Task SendTextFromQueue()
    {
        try
        {
            while (await _messagesTextToSendQueue.Reader.WaitToReadAsync())
            {
                while (_messagesTextToSendQueue.Reader.TryRead(out var message))
                {
                    try
                    {
                        await SendInternalSynchronized(message).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to send text message");
                    }
                }
            }
        }
        catch (TaskCanceledException) { }
        catch (OperationCanceledException) { }
        catch (Exception) when (_cancellationTotal!.IsCancellationRequested || _disposing) { }
        catch (Exception e)
        {
            _logger.LogTrace(e, "Sending text thread failed, Creating new thread");
            StartTextSendThread();
        }
    }

    /// <summary>
    /// Read the binary message queue and send messages to remote 
    /// </summary>
    private async Task SendBinaryFromQueue()
    {
        try
        {
            while (await _messagesBinaryToSendQueue.Reader.WaitToReadAsync())
            {
                while (_messagesBinaryToSendQueue.Reader.TryRead(out var message))
                {
                    try
                    {
                        await SendInternalSynchronized(message).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to binary text message");
                    }
                }
            }
        }
        catch (TaskCanceledException) { }
        catch (OperationCanceledException) { }
        catch (Exception) when (_cancellationTotal!.IsCancellationRequested || _disposing) { }
        catch (Exception e)
        {
            _logger.LogTrace(e, "Sending binary thread failed, Creating new thread");
            StartBinarySendThread();
        }
    }

    /// <summary>
    /// Start the Send thread for Text messages
    /// </summary>
    private void StartTextSendThread()
    {
        _ = Task.Factory.StartNew(_ => SendTextFromQueue(), TaskCreationOptions.LongRunning, _cancellationTotal!.Token);
    }

    /// <summary>
    /// Start the Send thread for Binary messages
    /// </summary>
    private void StartBinarySendThread()
    {
        _ = Task.Factory.StartNew(_ => SendBinaryFromQueue(), TaskCreationOptions.LongRunning, _cancellationTotal!.Token);
    }



    /// <summary>
    /// Send text message over the websocket, asynchronously locking it for thread safety
    /// </summary>
    /// <param name="message">UTF8 text message</param>
    private async Task SendInternalSynchronized(string message)
    {
        using (await _lock.LockAsync())
        {
            await SendInternal(message);
        }
    }
    /// <summary>
    /// Send binary message over the websocket, asynchronously locking it for thread safety
    /// </summary>
    /// <param name="message">UTF8 binary message</param>
    private async Task SendInternalSynchronized(ArraySegment<byte> message)
    {
        using (await _lock.LockAsync())
        {
            await SendInternal(message);
        }
    }

    /// <summary>
    /// Asynchronously send text message over the websocket
    /// </summary>
    /// <param name="message">UTF8 text message</param>
    private async Task SendInternal(string message)
    {
        if (!IsClientConnected())
        {
            _logger.LogDebug("Client not connected to remote, cant send message: [{message}]", message);
            return;
        }
        _logger.LogTrace("Sending: [{message}]", message);
        var messageBytes = MessageEncoding.GetBytes(message);
        await _client!.SendAsync(messageBytes, WebSocketMessageType.Text, true, _cancellation!.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously send binary message over the websocket
    /// </summary>
    /// <param name="message">UTF8 binary message</param>
    private async Task SendInternal(ArraySegment<byte> message)
    {
        if (!IsClientConnected())
        {
            _logger.LogDebug("Client not connected to remote, cant send message: [{message}]", message);
            return;
        }
        _logger.LogTrace("Sending: [{message}]", message);
        await _client!.SendAsync(message, WebSocketMessageType.Binary, true, _cancellation!.Token).ConfigureAwait(false);
    }
}
