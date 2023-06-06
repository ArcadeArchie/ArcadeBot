using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ArcadeBot.Net.Websockets.Models.Gateway.Events;


internal record HelloEvent(
    [property: JsonPropertyName("heartbeat_interval")] int Interval
);
