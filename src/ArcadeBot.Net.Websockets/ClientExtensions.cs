using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ArcadeBot.Net.Websockets.Models;

namespace ArcadeBot.Net.Websockets;

internal static class ClientExtensions
{
    public static Task SendInstant(this IDiscordWebSocketClient client, DiscordGatewayMessage message) =>
        client.SendInstant(JsonSerializer.Serialize(message));
}
