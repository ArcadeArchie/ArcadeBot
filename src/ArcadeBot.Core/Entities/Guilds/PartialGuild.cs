using System;
using ArcadeBot.Core.Utils;

namespace ArcadeBot.Core.Entities.Guilds
{
    public class PartialGuild : ISnowflakeEntity
    {
        public DateTimeOffset CreatedAt => SnowflakeUtils.FromId(Id);

        public ulong Id { get; internal set; }
        public string Name { get; internal set; } = null!;
        public string? Description { get; internal set; }
        public string SplashId { get; internal set; } = null!;
        public string? SplashUrl => CDNUtils.GetGuildSplashUrl(Id, SplashId);
        public string BannerId { get; internal set; } = null!;
        public string? BannerUrl => CDNUtils.GetGuildBannerUrl(Id, BannerId, ImageFormat.Auto);
        public GuildFeatures Features { get; internal set; } = null!;
        public string IconId { get; internal set; } = null!;
        public string? IconUrl => CDNUtils.GetGuildIconUrl(Id, IconId);
        public VerificationLevel? VerificationLevel { get; internal set; }
        public string? VanityURLCode { get; internal set; }
        public int? PremiumSubscriberCount { get; internal set; }
        public NsfwLevel? NsfwLevel { get; internal set; }

        public int? ApproxMemberCount { get; internal set; }
        public int? ApproxPresenceCount { get; internal set; }
    }
}