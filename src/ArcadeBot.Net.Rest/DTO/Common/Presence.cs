using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcadeBot.Core.Entities.Activities;
using ArcadeBot.Core.Entities.User;

namespace ArcadeBot.DTO
{
    public class Presence
    {
        public User? User { get; set; }
        public ulong? GuildId { get; set; }
        public UserStatus Status { get; set; }

        public ulong[]? Roles { get; set; }
        public string? Nickname { get; set; }
        public Dictionary<string, string>? ClientStatus { get; set; }
        public IEnumerable<Game> Activities { get; set; } = Enumerable.Empty<Game>();
        public DateTimeOffset? PremiumSince { get; set; }
    }
}