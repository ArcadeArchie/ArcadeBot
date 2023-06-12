using System.Text.Json;
using ArcadeBot.Net.WebSockets.Models;

namespace ArcadeBot.Net.WebSockets;

internal static class ClientExtensions
{
    public static Task SendInstant(this IDiscordWebSocketClient client, DiscordGatewayMessage message) =>
        client.SendInstant(JsonSerializer.Serialize(message));
}
