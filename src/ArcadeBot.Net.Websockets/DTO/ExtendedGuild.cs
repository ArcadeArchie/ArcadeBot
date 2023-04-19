using System.Text.Json.Serialization;

namespace ArcadeBot.DTO
{
    internal class ExtendedGuild : Guild
    {
        [JsonPropertyName("unavailable")]
        public bool? Unavailable { get; set; }

        [JsonPropertyName("member_count")]
        public int MemberCount { get; set; }

        [JsonPropertyName("large")]
        public bool Large { get; set; }

        [JsonPropertyName("presences")]
        public Presence[] Presences { get; set; } = Array.Empty<Presence>();

        [JsonPropertyName("members")]
        public GuildMember[] Members { get; set; } = Array.Empty<GuildMember>();

        [JsonPropertyName("channels")]
        public Channel[] Channels { get; set; } = Array.Empty<Channel>();

        [JsonPropertyName("joined_at")]
        public DateTimeOffset JoinedAt { get; set; }

        [JsonPropertyName("threads")]
        public new Channel[] Threads { get; set; } = Array.Empty<Channel>();

        [JsonPropertyName("guild_scheduled_events")]
        public GuildScheduledEvent[] GuildScheduledEvents { get; set; } = Array.Empty<GuildScheduledEvent>();
    }
}