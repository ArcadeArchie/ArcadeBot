using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcadeBot.Core.Entities.Emotes;
using ArcadeBot.Core.Entities.Guilds;
using ArcadeBot.Core.Entities.Permissions;

namespace ArcadeBot.Core.Entities.Roles;

public interface IRole : ISnowflakeEntity, IDeletable, IMentionable, IComparable<IRole>
{
    IGuild Guild { get; }
    Color Color { get; }
    bool IsHoisted { get; }
    bool IsManaged { get; }
    bool IsMentionable { get; }
    string Name { get; }
    string Icon { get; }
    Emoji Emoji { get; }
    GuildPermissions Permissions { get; }
    int Position { get; }
    RoleTags Tags { get; }
    
}
