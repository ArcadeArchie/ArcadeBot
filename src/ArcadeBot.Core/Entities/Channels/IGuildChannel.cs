using ArcadeBot.Core.Entities.Guilds;

namespace ArcadeBot.Core.Entities.Channels
{
    public interface IGuildChannel : IChannel, IDeletable
    {
        int Position { get; }

        ChannelFlags Flags { get; }
        IGuild Guild { get; }
        ulong GuildId { get; }
    }
}