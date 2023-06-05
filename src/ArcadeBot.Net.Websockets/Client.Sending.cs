using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Net.WebSockets;

namespace ArcadeBot.Net.Websockets;

internal partial class DiscordWebsocketClient
{
    private readonly Channel<string> _messagesTextToSendQueue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions()
    {
        SingleReader = true,
        SingleWriter = false
    });
    private readonly Channel<ArraySegment<byte>> _messagesBinaryToSendQueue = Channel.CreateUnbounded<ArraySegment<byte>>(new UnboundedChannelOptions()
    {
        SingleReader = true,
        SingleWriter = false
    });

    public void Send(string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        _messagesTextToSendQueue.Writer.TryWrite(message);
    }

    public void Send(byte[] message)
    {
        if (Equals(message, default(byte[])))
            throw new ArgumentNullException(nameof(message), "Message cant be null");
        _messagesBinaryToSendQueue.Writer.TryWrite(new ArraySegment<byte>(message));
    }

    public void Send(ArraySegment<byte> message)
    {
        if (Equals(message, default(ArraySegment<byte>)))
            throw new ArgumentNullException(nameof(message), "Message cant be null");
        _messagesBinaryToSendQueue.Writer.TryWrite(message);
    }

    public Task SendInstant(string message) => SendInternalSynchronized(message);

    public Task SendInstant(byte[] message) => SendInternalSynchronized(new ArraySegment<byte>(message));

    public void StreamFakeMessage(SocketResponse message) => _messageReceivedSubject.OnNext(message);



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

    private void StartTextSendThread()
    {
        _ = Task.Factory.StartNew(_ => SendTextFromQueue(), TaskCreationOptions.LongRunning, _cancellationTotal!.Token);
    }

    private void StartBinarySendThread()
    {
        _ = Task.Factory.StartNew(_ => SendBinaryFromQueue(), TaskCreationOptions.LongRunning, _cancellationTotal!.Token);
    }




    private async Task SendInternalSynchronized(string message)
    {
        using (await _lock.LockAsync())
        {
            await SendInternal(message);
        }
    }
    private async Task SendInternalSynchronized(ArraySegment<byte> message)
    {
        using (await _lock.LockAsync())
        {
            await SendInternal(message);
        }
    }

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
    private async Task SendInternal(ArraySegment<byte> message)
    {        
        if (!IsClientConnected())
        {
            _logger.LogDebug("Client not connected to remote, cant send message: [{message}]", message);
            return;
        }
        _logger.LogTrace("Sending: [{message}]", message);
        await _client!.SendAsync(message, WebSocketMessageType.Text, true, _cancellation!.Token).ConfigureAwait(false);
    }
}
