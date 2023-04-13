namespace ArcadeBot.Core.Entities.Channels
{
    public interface ITextChannel : IMessageChannel, IMentionable, INestedChannel, IIntergrationChannel
    {
    }
}