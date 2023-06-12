namespace ArcadeBot.Net.WebSockets.Models;

public class ReconnectionInfo
{
    /// <inheritdoc />
    public ReconnectionInfo(ReconnectionType type)
    {
        Type = type;
    }

    /// <summary>
    /// Reconnection reason
    /// </summary>
    public ReconnectionType Type { get; }

    /// <summary>
    /// Simple factory method
    /// </summary>
    public static ReconnectionInfo Create(ReconnectionType type)
    {
        return new ReconnectionInfo(type);
    }
}