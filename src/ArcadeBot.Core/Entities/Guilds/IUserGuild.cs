using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcadeBot.Core.Entities.Permissions;

namespace ArcadeBot.Core.Entities.Guilds
{
    public interface IUserGuild : IDeletable, ISnowflakeEntity
    {
        string Name { get; }
        string IconUrl { get; }
        bool IsOwner { get; }
        GuildPermissions Permissions { get; }
        GuildFeatures Features { get; }
        int? ApproxMemberCount { get; }
        int? ApproxPersenceCount { get; }
    }
}