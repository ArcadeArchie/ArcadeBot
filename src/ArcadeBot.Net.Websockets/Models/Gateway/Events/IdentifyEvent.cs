using System.Text.Json.Serialization;
using ArcadeBot.Core;

namespace ArcadeBot.Net.WebSockets.Models.Gateway.Events;

internal class IdentifyEvent
{
    private static readonly GatewayIntents _defaultIntents = (GatewayIntents.AllUnprivileged & ~(GatewayIntents.GuildScheduledEvents | GatewayIntents.GuildInvites)) |
            GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.MessageContent;


    [JsonPropertyName("token")]
    public string Token { get; init; } = null!;
    
    [JsonPropertyName("intents")]
    public GatewayIntents Intents { get; init; } = _defaultIntents;
    
    [JsonPropertyName("presence")]
    public PresenceUpdateParams Presence { get; } = new("dnd", 0, false, new List<Activity>
    {
        new Activity(0, 0, "Being reworked from scratch", "Being reworked from scratch")
    });
    
    [JsonPropertyName("compress")]
    public bool UseMessageCompression = false;
    
    [JsonPropertyName("large_threshold")]
    public int LargeThreshold { get; init; } = 50;

    [JsonPropertyName("shard")]
    public int[] ShardingParams { get; init; } = null!;

    [JsonPropertyName("properties")]
    public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>
    {
        ["os"] = "Microsoft Shitbox",
        ["browser"] = ".NET",
        ["device"] = ".NET"
    };

    private IdentifyEvent() { }
    public static IdentifyEvent FromOptions(BotOptions options)
    {
        var output = new IdentifyEvent
        {
            Token = options.Token,
            UseMessageCompression = true,
            ShardingParams = new[] { options.ShardId ?? 0, options.TotalShards ?? 1 },
        };
        return output;
    }
}

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