using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcadeBot.Core.Entities.Roles;

namespace ArcadeBot.Core.Entities.Guilds;

public interface IGuild : IDeletable, ISnowflakeEntity
{
    string Name { get; }
    int AFKTimeout { get; }
    bool IsWidgetEnable { get; }
    DefaultMessageNotifications DefaultMessageNotifications { get; }
    MfaLevel MfaLevel { get; }
    VerificationLevel VerificationLevel { get; }
    ExplicitContentFilterLevel ExplicitContentFilter { get; }
    string IconId { get; }
    string IconUrl { get; }
    string SplashId { get; }
    string SplashUrl { get; }
    string DiscoverySplashId { get; }
    string DiscoverySplashUrl { get; }
    bool Available { get; }
    ulong? AFKChannelId { get; }
    ulong? WidgetChannelId { get; }
    ulong? SafetyAlertsChannelId { get; }
    ulong? SystemChannelId { get; }
    ulong? RulesChannelId { get; }
    ulong? ApplicationId { get; }
    string VoiceRegionId { get; }
    IRole EveryoneRole { get; }
}
