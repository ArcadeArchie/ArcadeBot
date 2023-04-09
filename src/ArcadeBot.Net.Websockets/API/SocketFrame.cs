using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ArcadeBot.Net.WebSockets.API
{
    internal class SocketFrame
    {
        [JsonPropertyName("t")]
        public string? EventName { get; init; }
        [JsonPropertyName("s")]
        public int? Sequence { get; init; }
        [JsonPropertyName("op")]
        public OpCodes.Gateway OpCode { get; init; }
        [JsonPropertyName("d")]
        public JsonNode? EventData { get; init; }
    }
}