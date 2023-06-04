using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeBot.Core.Entities.Activities;

public interface IActivity
{
    string Name { get; }
    ActivityType Type { get; }
    ActivityProperties Flags { get; }
    string? Details { get; }
}
