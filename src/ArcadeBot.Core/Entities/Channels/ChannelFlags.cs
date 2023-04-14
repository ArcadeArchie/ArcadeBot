namespace ArcadeBot.Core.Entities.Channels;

public enum ChannelFlags
{
    None = 0,
    Pinned = 1 << 1,
    RequireTag = 1 << 4
}
