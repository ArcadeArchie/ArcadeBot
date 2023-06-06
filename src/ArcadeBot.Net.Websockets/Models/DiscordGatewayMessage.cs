using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ArcadeBot.DTO;

namespace ArcadeBot.Net.Websockets.Models;

public class DiscordGatewayMessage
{
    [JsonPropertyName("t")]
    public string? EventName { get; init; }
    [JsonPropertyName("s")]
    public int? Sequence { get; init; }
    [JsonPropertyName("op")]
    public OpCodes.Gateway OpCode { get; init; }
    [JsonPropertyName("d")]
    public JsonNode? EventData { get; init; }


    public override string ToString()
    {
        return string.Format("eventName: {0}, seq: {1}, opCode: {2}, jsonData: {3}", EventName, Sequence, OpCode, EventData?.ToJsonString() ?? "");
    }
}
