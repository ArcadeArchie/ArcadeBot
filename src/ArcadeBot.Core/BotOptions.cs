using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeBot.Core;

public class BotOptions
{
    public string Token { get; set; } = null!;
    public int? ShardId { get; set; }
    public int? TotalShards { get; set; }
}
