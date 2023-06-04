using System.Text.Json.Serialization;
using ArcadeBot.DTO;

namespace ArcadeBot.DTO.Gateway
{
    internal class ReadyEvent
    {
        public class ReadState
        {
            [JsonPropertyName("id")]
            public string? ChannelId { get; set; }
            [JsonPropertyName("mention_count")]
            public int MentionCount { get; set; }
            [JsonPropertyName("last_message_id")]
            public string? LastMessageId { get; set; }
        }

        [JsonPropertyName("v")]
        int Version { get; set; }

        [JsonPropertyName("user")]
        public User? User { get; set; }
        [JsonPropertyName("session_id")]
        public string? SessionId { get; set; }
        [JsonPropertyName("resume_gateway_url")]
        public string? ResumeGatewayUrl { get; set; }
        [JsonPropertyName("read_state")]
        public ReadState[] ReadStates { get; set; } = Array.Empty<ReadState>();
        [JsonPropertyName("guilds")]
        public ExtendedGuild[] Guilds { get; set; } = Array.Empty<ExtendedGuild>();
        [JsonPropertyName("private_channels")]
        public Channel[] PrivateChannels { get; set; } = Array.Empty<Channel>();
        [JsonPropertyName("relationships")]
        public Relationship[] Relationships { get; set; } = Array.Empty<Relationship>();
        [JsonPropertyName("application")]
        public PartialApplication? Application { get; set; }
    }
}