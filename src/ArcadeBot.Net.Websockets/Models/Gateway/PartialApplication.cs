using System.Text.Json.Serialization;
using ArcadeBot.Core.Entities.Applications;

namespace ArcadeBot.Net.WebSockets.Models.Gateway;

public class PartialApplication
{
    [JsonPropertyName("id")]
    public ulong Id { get; set; }
    [JsonPropertyName("flags")]
    public ApplicationFlags Flags { get; set; }
}
