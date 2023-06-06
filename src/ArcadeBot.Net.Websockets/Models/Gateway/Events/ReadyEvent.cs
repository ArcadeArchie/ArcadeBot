using System.Text.Json.Serialization;
using ArcadeBot.DTO;

namespace ArcadeBot.Net.Websockets.Models.Gateway.Events;

internal class ReadyEvent
{
    [JsonPropertyName("v")]
    int Version { get; set; }

    [JsonPropertyName("user")]
    public User? User { get; set; }

    [JsonPropertyName("guilds")]
    public PartialGuild[] Guilds { get; set; } = Array.Empty<PartialGuild>();

    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }

    [JsonPropertyName("resume_gateway_url")]
    public string? ResumeGatewayUrl { get; set; }
    
    [JsonPropertyName("application")]
    public PartialApplication? Application { get; set; }
}
