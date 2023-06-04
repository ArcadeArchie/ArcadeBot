using ArcadeBot.Core.Entities.User;

namespace ArcadeBot.Core.Entities.Guilds
{
    public interface IBan
    {
        IUser User { get; }
        string Reason { get; }
    }
}