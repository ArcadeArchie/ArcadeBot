using ArcadeBot.Core.Entities.Channels;

namespace ArcadeBot.Core.Entities.User
{
    public interface IVoiceState
    {
        bool IsDeafend { get; }
        bool IsMuted { get; }
        bool IsSelfDeafend { get; }
        bool IsSelfMuted { get; }
        bool IsSuppressed { get; }
        IVoiceChannel VoiceChannel { get; }
        string VoiceSessionId { get; }
        bool IsStreaming { get; }
        bool IsVideoing { get; }
        DateTimeOffset? RequestToSpeakTimestamp { get; }

    }
}