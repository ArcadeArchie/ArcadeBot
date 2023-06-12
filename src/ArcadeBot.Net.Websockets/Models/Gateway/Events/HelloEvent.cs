using System.Text.Json.Serialization;

namespace ArcadeBot.Net.WebSockets.Models.Gateway.Events;


internal record HelloEvent(
    [property: JsonPropertyName("heartbeat_interval")] int Interval
);
