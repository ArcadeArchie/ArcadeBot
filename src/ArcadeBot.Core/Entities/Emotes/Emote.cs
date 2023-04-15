using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcadeBot.Core.Utils;

namespace ArcadeBot.Core.Entities.Emotes
{
    public class Emote : IEmote, ISnowflakeEntity
    {
        public ulong Id { get; }
        public string Name { get; }

        public bool Animated { get; }
        public DateTimeOffset CreatedAt => SnowflakeUtils.FromId(Id);
        public string Url => CDNUtils.GetEmoteUri(Id, Animated);
    }
}