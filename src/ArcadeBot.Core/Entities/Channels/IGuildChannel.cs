namespace ArcadeBot.Core.Entities.Channels
{
    public interface IGuildChannel : IChannel, IDeletable
    {
        int Position { get; }
        
    }
}