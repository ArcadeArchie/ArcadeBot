using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcadeBot.Core.Entities.Activities;

namespace ArcadeBot.Core.Entities.User;

public interface IPresence
{
    UserStatus Status { get; }
    IReadOnlyCollection<ClientType> ActiveClients { get; }
    IReadOnlyCollection<IActivity> Activities { get; }
}
