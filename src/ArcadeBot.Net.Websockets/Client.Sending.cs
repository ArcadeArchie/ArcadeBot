using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Channels;

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

    public Task SendInstant(string message)
    {
        throw new NotImplementedException();
    }

    public Task SendInstant(byte[] message)
    {
        throw new NotImplementedException();
    }

    public void StreamFakeMessage(SocketResponse message)
    {
        throw new NotImplementedException();
    }



    private async Task SendTextFromQueue()
    { }
    private async Task SendBinaryFromQueue()
    { }
    private void StartTextSendThread()
    {
        _ = Task.Factory.StartNew(_ => SendTextFromQueue(), TaskCreationOptions.LongRunning, _cancellationTotal!.Token);
    }

    private void StartBinarySendThread()
    {
        _ = Task.Factory.StartNew(_ => SendBinaryFromQueue(), TaskCreationOptions.LongRunning, _cancellationTotal!.Token);
    }
}
