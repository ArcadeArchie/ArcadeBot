using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeBot.Core.Entities.Activities
{
    [Flags]
    public enum ActivityProperties
    {
        None = 0,
        Instance = 1,
        Join = 0b10,
        Spectate = 0b100,
        JoinRequest = 0b1000,
        Sync = 0b10000,
        Play = 0b100000,
        PartyPrivacyFriends = 0b1000000,
        PartyPrivacyVoiceChannel = 0b10000000,
        Embedded = 0b100000000,
    }
}