using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeBot.Core.Utils
{
    public static class CDNUtils
    {
        public static string GetEmoteUri(ulong id, bool isAnimated) => $"{Constants.CDNUrl}emojis/{id}.{(isAnimated ? "gif" : "png")}";
    }
}