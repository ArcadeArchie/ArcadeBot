namespace ArcadeBot.Core.Entities.Channels
{
    public interface INestedChannel : IGuildChannel
    {
        ulong? Categoryid { get; }
    }
}