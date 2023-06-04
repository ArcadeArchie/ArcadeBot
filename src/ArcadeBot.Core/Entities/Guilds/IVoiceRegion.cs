using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeBot.Core.Entities.Guilds
{
    public interface IVoiceRegion
    {
        string Id { get; }
        string Name { get; }
        bool IsVip { get; }
        bool IsOptimal { get; }
        bool IsDepricated { get; }
        bool IsCustom { get; }
    }
}