namespace ArcadeBot.Core.Entities.Channels
{
    public interface ITextChannel : IMessageChannel, IMentionable, INestedChannel, IIntergrationChannel
    {
        bool IsNsfw { get; }
        string Topic { get; }
        int SlowModeInterval { get; }
        int DefaultSlowModeInterval { get; }
        ThreadArchiveDuration DefaultArchiveDuration { get; }
    }
}