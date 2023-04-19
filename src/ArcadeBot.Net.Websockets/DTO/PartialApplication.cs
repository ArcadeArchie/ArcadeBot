using System.Text.Json.Serialization;
using ArcadeBot.Core.Entities.Applications;

namespace ArcadeBot.DTO
{
    internal class PartialApplication
    {
        [JsonPropertyName("id")]
        public ulong Id { get; set; }
        [JsonPropertyName("flags")]
        public ApplicationFlags Flags { get; set; }
    }
}