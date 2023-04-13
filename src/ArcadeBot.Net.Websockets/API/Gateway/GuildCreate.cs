using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ArcadeBot.Net.Websockets.API.Gateway
{

    public class GuildeCreate
    {
        [JsonPropertyName("application_command_counts")]
        public Dictionary<string, long> ApplicationCommandCounts { get; set; } = null!;

        [JsonPropertyName("member_count")]
        public long MemberCount { get; set; }

        [JsonPropertyName("description")]
        public object Description { get; set; } = null!;

        [JsonPropertyName("channels")]
        public Channel[] Channels { get; set; } = null!;

        [JsonPropertyName("system_channel_id")]
        public object SystemChannelId { get; set; } = null!;

        [JsonPropertyName("verification_level")]
        public long VerificationLevel { get; set; }

        [JsonPropertyName("max_stage_video_channel_users")]
        public long MaxStageVideoChannelUsers { get; set; }

        [JsonPropertyName("members")]
        public Member[] Members { get; set; } = null!;

        [JsonPropertyName("region")]
        public string Region { get; set; } = null!;

        [JsonPropertyName("nsfw")]
        public bool Nsfw { get; set; }

        [JsonPropertyName("banner")]
        public object Banner { get; set; } = null!;

        [JsonPropertyName("discovery_splash")]
        public object DiscoverySplash { get; set; } = null!;

        [JsonPropertyName("rules_channel_id")]
        public string RulesChannelId { get; set; } = null!;

        [JsonPropertyName("system_channel_flags")]
        public long SystemChannelFlags { get; set; }

        [JsonPropertyName("mfa_level")]
        public long MfaLevel { get; set; }

        [JsonPropertyName("voice_states")]
        public VoiceState[] VoiceStates { get; set; } = null!;

        [JsonPropertyName("presences")]
        public object[] Presences { get; set; } = null!;

        [JsonPropertyName("guild_scheduled_events")]
        public object[] GuildScheduledEvents { get; set; } = null!;

        [JsonPropertyName("premium_progress_bar_enabled")]
        public bool PremiumProgressBarEnabled { get; set; }

        [JsonPropertyName("default_message_notifications")]
        public long DefaultMessageNotifications { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("max_members")]
        public long MaxMembers { get; set; }

        [JsonPropertyName("emojis")]
        public Emoji[] Emojis { get; set; } = null!;

        [JsonPropertyName("safety_alerts_channel_id")]
        public string SafetyAlertsChannelId { get; set; } = null!;

        [JsonPropertyName("public_updates_channel_id")]
        public string PublicUpdatesChannelId { get; set; } = null!;

        [JsonPropertyName("max_video_channel_users")]
        public long MaxVideoChannelUsers { get; set; }

        [JsonPropertyName("application_id")]
        public object ApplicationId { get; set; } = null!;

        [JsonPropertyName("guild_hashes")]
        public GuildHashes GuildHashes { get; set; } = null!;

        [JsonPropertyName("hub_type")]
        public object HubType { get; set; } = null!;

        [JsonPropertyName("stage_instances")]
        public object[] StageInstances { get; set; } = null!;

        [JsonPropertyName("threads")]
        public object[] Threads { get; set; } = null!;

        [JsonPropertyName("joined_at")]
        public DateTimeOffset JoinedAt { get; set; }

        [JsonPropertyName("premium_subscription_count")]
        public long PremiumSubscriptionCount { get; set; }

        [JsonPropertyName("explicit_content_filter")]
        public long ExplicitContentFilter { get; set; }

        [JsonPropertyName("embedded_activities")]
        public object[] EmbeddedActivities { get; set; } = null!;

        [JsonPropertyName("owner_id")]
        public string OwnerId { get; set; } = null!;

        [JsonPropertyName("premium_tier")]
        public long PremiumTier { get; set; }

        [JsonPropertyName("splash")]
        public object Splash { get; set; } = null!;

        [JsonPropertyName("nsfw_level")]
        public long NsfwLevel { get; set; }

        [JsonPropertyName("roles")]
        public Role[] Roles { get; set; } = null!;

        [JsonPropertyName("stickers")]
        public object[] Stickers { get; set; } = null!;

        [JsonPropertyName("features")]
        public string[] Features { get; set; } = null!;

        [JsonPropertyName("home_header")]
        public object HomeHeader { get; set; } = null!;

        [JsonPropertyName("large")]
        public bool Large { get; set; }

        [JsonPropertyName("latest_onboarding_question_id")]
        public string LatestOnboardingQuestionId { get; set; } = null!;

        [JsonPropertyName("unavailable")]
        public bool Unavailable { get; set; }

        [JsonPropertyName("afk_channel_id")]
        public object AfkChannelId { get; set; } = null!;

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = null!;

        [JsonPropertyName("lazy")]
        public bool Lazy { get; set; }

        [JsonPropertyName("afk_timeout")]
        public long AfkTimeout { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("preferred_locale")]
        public string PreferredLocale { get; set; } = null!;

        [JsonPropertyName("vanity_url_code")]
        public object VanityUrlCode { get; set; } = null!;
    }

    public class Channel
    {
        [JsonPropertyName("version")]
        public long Version { get; set; }

        [JsonPropertyName("type")]
        public long Type { get; set; }

        [JsonPropertyName("position")]
        public long Position { get; set; }

        [JsonPropertyName("permission_overwrites")]
        public PermissionOverwrite[] PermissionOverwrites { get; set; } = null!;

        [JsonPropertyName("parent_id")]
        public string ParentId { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("flags")]
        public long Flags { get; set; }

        [JsonPropertyName("topic")]
        public string Topic { get; set; } = null!;

        [JsonPropertyName("rate_limit_per_user")]
        public long? RateLimitPerUser { get; set; }

        [JsonPropertyName("nsfw")]
        public bool? Nsfw { get; set; }

        [JsonPropertyName("last_message_id")]
        public string LastMessageId { get; set; } = null!;

        [JsonPropertyName("user_limit")]
        public long? UserLimit { get; set; }

        [JsonPropertyName("rtc_region")]
        public object RtcRegion { get; set; } = null!;

        [JsonPropertyName("bitrate")]
        public long? Bitrate { get; set; }

        [JsonPropertyName("video_quality_mode")]
        public long? VideoQualityMode { get; set; }

        [JsonPropertyName("template")]
        public string Template { get; set; } = null!;

        [JsonPropertyName("default_sort_order")]
        public object DefaultSortOrder { get; set; } = null!;

        [JsonPropertyName("default_reaction_emoji")]
        public DefaultReactionEmoji DefaultReactionEmoji { get; set; } = null!;

        [JsonPropertyName("default_forum_layout")]
        public long? DefaultForumLayout { get; set; }

        [JsonPropertyName("available_tags")]
        public AvailableTag[] AvailableTags { get; set; } = null!;
    }

    public class AvailableTag
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("moderated")]
        public bool Moderated { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("emoji_name")]
        public object EmojiName { get; set; } = null!;

        [JsonPropertyName("emoji_id")]
        public object EmojiId { get; set; } = null!;
    }

    public class DefaultReactionEmoji
    {
        [JsonPropertyName("emoji_name")]
        public object EmojiName { get; set; } = null!;

        [JsonPropertyName("emoji_id")]
        public string EmojiId { get; set; } = null!;
    }

    public class PermissionOverwrite
    {
        [JsonPropertyName("type")]
        public long Type { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("deny")]
        public string Deny { get; set; } = null!;

        [JsonPropertyName("allow")]
        public string Allow { get; set; } = null!;
    }

    public class Emoji
    {
        [JsonPropertyName("version")]
        public long Version { get; set; }

        [JsonPropertyName("roles")]
        public string[] Roles { get; set; } = null!;

        [JsonPropertyName("require_colons")]
        public bool RequireColons { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("managed")]
        public bool Managed { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("available")]
        public bool Available { get; set; }

        [JsonPropertyName("animated")]
        public bool Animated { get; set; }
    }

    public class GuildHashes
    {
        [JsonPropertyName("version")]
        public long Version { get; set; }

        [JsonPropertyName("roles")]
        public Channels Roles { get; set; } = null!;

        [JsonPropertyName("metadata")]
        public Channels Metadata { get; set; } = null!;

        [JsonPropertyName("channels")]
        public Channels Channels { get; set; } = null!;
    }

    public class Channels
    {
        [JsonPropertyName("omitted")]
        public bool Omitted { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; } = null!;
    }

    public class Member
    {
        [JsonPropertyName("user")]
        public User User { get; set; } = null!;

        [JsonPropertyName("roles")]
        public string[] Roles { get; set; } = null!;

        [JsonPropertyName("premium_since")]
        public object PremiumSince { get; set; } = null!;

        [JsonPropertyName("pending")]
        public bool Pending { get; set; }

        [JsonPropertyName("nick")]
        public string Nick { get; set; } = null!;

        [JsonPropertyName("mute")]
        public bool Mute { get; set; }

        [JsonPropertyName("joined_at")]
        public DateTimeOffset JoinedAt { get; set; }

        [JsonPropertyName("flags")]
        public long Flags { get; set; }

        [JsonPropertyName("deaf")]
        public bool Deaf { get; set; }

        [JsonPropertyName("communication_disabled_until")]
        public object CommunicationDisabledUntil { get; set; } = null!;

        [JsonPropertyName("avatar")]
        public object Avatar { get; set; } = null!;
    }

    public class User
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = null!;

        [JsonPropertyName("public_flags")]
        public long PublicFlags { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("display_name")]
        public object DisplayName { get; set; } = null!;

        [JsonPropertyName("discriminator")]
        public string Discriminator { get; set; } = null!;

        [JsonPropertyName("bot")]
        public bool Bot { get; set; }

        [JsonPropertyName("avatar_decoration")]
        public object AvatarDecoration { get; set; } = null!;

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; } = null!;
    }

    public class Role
    {
        [JsonPropertyName("version")]
        public long Version { get; set; }

        [JsonPropertyName("unicode_emoji")]
        public object UnicodeEmoji { get; set; } = null!;

        [JsonPropertyName("tags")]
        public Tags Tags { get; set; } = null!;

        [JsonPropertyName("position")]
        public long Position { get; set; }

        [JsonPropertyName("permissions")]
        public string Permissions { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("mentionable")]
        public bool Mentionable { get; set; }

        [JsonPropertyName("managed")]
        public bool Managed { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("icon")]
        public object Icon { get; set; } = null!;

        [JsonPropertyName("hoist")]
        public bool Hoist { get; set; }

        [JsonPropertyName("flags")]
        public long Flags { get; set; }

        [JsonPropertyName("color")]
        public long Color { get; set; }
    }

    public class Tags
    {
        [JsonPropertyName("premium_subscriber")]
        public object PremiumSubscriber { get; set; } = null!;

        [JsonPropertyName("bot_id")]
        public string BotId { get; set; } = null!;

        [JsonPropertyName("integration_id")]
        public string IntegrationId { get; set; } = null!;
    }

    public class VoiceState
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = null!;

        [JsonPropertyName("suppress")]
        public bool Suppress { get; set; }

        [JsonPropertyName("session_id")]
        public string SessionId { get; set; } = null!;

        [JsonPropertyName("self_video")]
        public bool SelfVideo { get; set; }

        [JsonPropertyName("self_mute")]
        public bool SelfMute { get; set; }

        [JsonPropertyName("self_deaf")]
        public bool SelfDeaf { get; set; }

        [JsonPropertyName("request_to_speak_timestamp")]
        public object RequestToSpeakTimestamp { get; set; } = null!;

        [JsonPropertyName("mute")]
        public bool Mute { get; set; }

        [JsonPropertyName("deaf")]
        public bool Deaf { get; set; }

        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; } = null!;
    }

}