using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ArcadeBot.Core.Entities.Applications;

namespace ArcadeBot.Net.Websockets.Models.Gateway;

public class PartialApplication
{
    [JsonPropertyName("id")]
    public ulong Id { get; set; }
    [JsonPropertyName("flags")]
    public ApplicationFlags Flags { get; set; }
}
