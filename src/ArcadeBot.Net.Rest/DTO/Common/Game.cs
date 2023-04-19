using System.Text.Json.Serialization;
using ArcadeBot.Core.Entities.Activities;

namespace ArcadeBot.DTO
{
    public class Game
    {        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("url")]
        public string? StreamUrl { get; set; }
        [JsonPropertyName("type")]
        public ActivityType? Type { get; set; }
        [JsonPropertyName("details")]
        public string? Details { get; set; }
        [JsonPropertyName("state")]
        public string? State { get; set; }
        [JsonPropertyName("application_id")]
        public ulong? ApplicationId { get; set; }
        [JsonPropertyName("assets")]
        public DTO.GameAssets? Assets { get; set; }
        [JsonPropertyName("party")]
        public DTO.GameParty? Party { get; set; }
        [JsonPropertyName("secrets")]
        public DTO.GameSecrets? Secrets { get; set; }
        [JsonPropertyName("timestamps")]
        public DTO.GameTimestamps? Timestamps { get; set; }
        [JsonPropertyName("instance")]
        public bool? Instance { get; set; }
        [JsonPropertyName("sync_id")]
        public string? SyncId { get; set; }
        [JsonPropertyName("session_id")]
        public string? SessionId { get; set; }
        [JsonPropertyName("Flags")]
        public ActivityProperties? Flags { get; set; }
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("emoji")]
        public Emoji? Emoji { get; set; }
        [JsonPropertyName("created_at")]
        public long? CreatedAt { get; set; }
    }
}