using System.Text.Json.Serialization;

namespace ArcadeBot.DTO.Gateway
{
    internal class Identify
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }
        [JsonPropertyName("properties")]
        public IDictionary<string, string>? Properties { get; set; }
        [JsonPropertyName("large_threshold")]
        public int LargeThreshold { get; set; }
        [JsonPropertyName("shard")]
        public int[]? ShardingParams { get; set; }
        [JsonPropertyName("presence")]
        public PresenceUpdateParams? Presence { get; set; }
        [JsonPropertyName("intents")]
        public int Intents { get; set; }
    }
    internal class PresenceUpdateParams

    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
        [JsonPropertyName("since")]
        public long? IdleSince { get; set; }
        [JsonPropertyName("afk")]
        public bool IsAFK { get; set; }
        [JsonPropertyName("activities")]
        public List<Activity>? Activities { get; set; } // TODO, change to interface later
    }
    internal class Activity
    {
        [JsonPropertyName("type")]
        public int Type { get; set; }
        [JsonPropertyName("flags")]
        public int Flags { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("details")]
        public string? Details {get;set;}
    }
}