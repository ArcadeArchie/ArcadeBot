namespace ArcadeBot.Core.Entities.Channels
{
    public interface IChannel : ISnowflakeEntity
    {
        string Name { get; }

    }
}