using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ArcadeBot.DTO;

namespace ArcadeBot.Net.Websockets.Models.Gateway;

public sealed class PartialGuild : Guild
{
    [JsonPropertyName("unavailable")]
    public bool? Unavailable { get; set; }
}
