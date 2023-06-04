using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeBot.Core.Entities.User;

public interface IUser : ISnowflakeEntity, IMentionable, IPresence
{
    string AvatarId { get; }
    string Discriminator { get; }
    string DiscriminatorValue { get; }
    bool IsBot { get; }
    bool IsWebhook { get; }
    string Username { get; }
    UserProperties? PublicFlags { get; }
}

