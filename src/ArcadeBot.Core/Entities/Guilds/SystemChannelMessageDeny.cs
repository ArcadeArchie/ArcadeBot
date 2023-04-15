using System;

namespace ArcadeBot.Core.Entities.Guilds
{
    [Flags]
    public enum SystemChannelMessageDeny
    {
        None = 0,
        WelcomeMessage = 1 << 0,
        GuildBoost = 1 << 1,
        GuildSetupTip = 1 << 2,
        WelcomeMessageReply = 1 << 3,
        RoleSubscriptionPurchase = 1 << 4,
        RoleSubscriptionPurchaseReplies = 1 << 5
    }
}