using System.Net.WebSockets;

namespace ArcadeBot.Net.Websockets;

public class SocketResponse
{

    /// <summary>
    /// Received text message
    /// </summary>
    /// <remarks>only set if type = WebSocketMessageType.Text</remarks>
    public string? Text { get; }

    /// <summary>
    /// Received text message
    /// </summary>
    /// <remarks>only set if type = WebSocketMessageType.Binary</remarks>
    public byte[]? Binary { get; }

    /// <summary>
    /// Current message type (Text or Binary)
    /// </summary>
    public WebSocketMessageType MessageType { get; }


    private SocketResponse(byte[]? binary, string? text, WebSocketMessageType messageType)
    {
        Binary = binary;
        Text = text;
        MessageType = messageType;
    }
    /// <summary>
    /// Return string info about the message
    /// </summary>
    public override string ToString()
    {
        if (MessageType == WebSocketMessageType.Text)
        {
            return Text!;
        }

        return $"Type binary, length: {Binary?.Length}";
    }

    /// <summary>
    /// Create text response message
    /// </summary>
    public static SocketResponse TextMessage(string? data)
    {
        return new SocketResponse(null, data, WebSocketMessageType.Text);
    }

    /// <summary>
    /// Create binary response message
    /// </summary>
    public static SocketResponse BinaryMessage(byte[]? data)
    {
        return new SocketResponse(data, null, WebSocketMessageType.Binary);
    }
}
