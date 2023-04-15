using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeBot.Core.Entities;

public interface IEntity<TId> where TId : IEquatable<TId>
{
    TId Id { get; }
}
