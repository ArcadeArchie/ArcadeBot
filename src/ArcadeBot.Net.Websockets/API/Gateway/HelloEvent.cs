using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ArcadeBot.Net.Websockets.API.Gateway
{
    public class HelloEvent
    {        
        [JsonPropertyName("heartbeat_interval")]
        public int Interval { get; init; }
    }
}