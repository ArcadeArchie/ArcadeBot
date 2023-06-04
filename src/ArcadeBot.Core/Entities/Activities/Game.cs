using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeBot.Core.Entities.Activities
{
    public class Game : IActivity
    {
        public string Name { get; set; }
        public ActivityType Type { get; set; }
        public ActivityProperties Flags { get; set; }
        public string? Details { get; set; }

        public Game(
            string name, string? details = null, 
            ActivityType type = ActivityType.Playing, ActivityProperties flags = ActivityProperties.Instance)
        {
            Name = name;
            Details = details;
            Type = type;
            Flags = flags;
        }
    }
}