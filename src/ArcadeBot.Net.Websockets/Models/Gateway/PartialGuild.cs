using System.Text.Json.Serialization;
using ArcadeBot.DTO;

namespace ArcadeBot.Net.WebSockets.Models.Gateway;

public sealed class PartialGuild : Guild
{
    [JsonPropertyName("unavailable")]
    public bool? Unavailable { get; set; }
}
