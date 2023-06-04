using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ArcadeBot.Core.Entities.Emotes;
using ArcadeBot.Core.Entities.Roles;
using ArcadeBot.Core.Entities.Stickers;

namespace ArcadeBot.Core.Entities.Guilds;

public interface IGuild : IDeletable, ISnowflakeEntity
{
    string Name { get; }
    int AFKTimeout { get; }

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

    int PremiumSubcriptionCount { get; }

    string BannerId { get; }
    string BannerUrl { get; }
    string VanityUrlCode { get; }
    string Description { get; }
    int? AproxMemberCount { get; }
    int? AproxPresenceCount { get; }
    string PreferredLocale { get; }
    CultureInfo PreferredCulture { get; }
    
    #region Limits

    ulong MaxUploadLimit { get; }
    int MaxBitrate { get; }
    int? MaxPresences { get; }
    int? MaxMembers { get; }
    int? MaxVideoChannelUsers { get; }
    int? MaxStageVideoChannelUsers { get; }
        
    #endregion

    #region Flags
    bool IsBoostProgressEnabled { get; }
    bool IsWidgetEnable { get; }

    NsfwLevel NsfwLevel { get; }
    PremiumTier PremiumTier { get; }
    GuildFeatures Features { get; }
    SystemChannelMessageDeny SystemChannelFlags { get; }
    DefaultMessageNotifications DefaultMessageNotifications { get; }
    MfaLevel MfaLevel { get; }
    VerificationLevel VerificationLevel { get; }
    ExplicitContentFilterLevel ExplicitContentFilter { get; }
        
    #endregion

    IReadOnlyCollection<GuildEmote> Emotes { get; }
    IReadOnlyCollection<ICustomSticker> Stickers { get; }
    IReadOnlyCollection<IRole> Roles { get; }
}
