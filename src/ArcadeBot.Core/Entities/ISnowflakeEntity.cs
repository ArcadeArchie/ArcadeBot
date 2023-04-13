using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeBot.Core.Entities;

public interface ISnowflakeEntity : IEntity<ulong>
{
    DateTimeOffset CreatedAt { get; }
}
