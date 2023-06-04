using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeBot.Core.Entities.Permissions;
[DebuggerDisplay(@"{DebuggerDisplay,nq}")]
public struct GuildPermissions
{
    public ulong RawValue { get; }


    public GuildPermissions(ulong rawValue) => RawValue = rawValue;
    public GuildPermissions(string rawValue) => RawValue = ulong.Parse(rawValue);
}
