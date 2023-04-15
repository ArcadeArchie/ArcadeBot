using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeBot.Core.Utils
{
    public static class SnowflakeUtils
    {
        public static DateTimeOffset FromId(ulong id) => DateTimeOffset.FromUnixTimeMilliseconds((long)((id >> 22) + 1420070400000UL));
        public static ulong ToId(DateTimeOffset value)
            => ((ulong)value.ToUnixTimeMilliseconds() - 1420070400000UL) << 22;
    }
}