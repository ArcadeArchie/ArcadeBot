using System.Text.Json.Serialization;
using ArcadeBot.Net.WebSockets;

namespace ArcadeBot.Net.Websockets.Models.Gateway.Events;

internal record IdentifyEvent(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("properties")] IDictionary<string, string> Properties,
    [property: JsonPropertyName("intents")] GatewayIntents Intents,
    [property: JsonPropertyName("compress")] bool UseMessageCompression = false,
    [property: JsonPropertyName("large_threshold")] int LargeThreshold = 50,
    [property: JsonPropertyName("shard")] int[]? ShardingParams = null,
    [property: JsonPropertyName("presence")] PresenceUpdateParams? Presence = null
);

internal record PresenceUpdateParams(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("since")] long IdleSince,
    [property: JsonPropertyName("afk")] bool IsAFK,
    [property: JsonPropertyName("activities")] IEnumerable<Activity>? Activities
);
internal record Activity(
    [property: JsonPropertyName("type")] int Type,
    [property: JsonPropertyName("flags")] int Flags,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("details")] string? Details
);