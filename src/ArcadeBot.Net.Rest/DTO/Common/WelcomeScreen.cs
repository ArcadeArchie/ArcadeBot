using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeBot.DTO;

public class WelcomeScreen
{
    public string? Description { get; set; }
    public WelcomeScreenChannel[] WelcomeChannels { get; set; } = Array.Empty<WelcomeScreenChannel>();
}
