namespace ArcadeBot.Core.Entities.Applications
{
    public enum ApplicationFlags
    {
        UsesAutoModApi = 1 << 6,
        GatewayPresence = 1 << 12,
        GatewayPresenceLimited = 1 << 13,
        GatewayGuildMembers = 1 << 14,
        GatewayGuildMembersLimited = 1 << 15,
        VerificationPendingGuildLimit = 1 << 16,
        Embedded = 1 << 17,
        GatewayMessageContent = 1 << 18,
        GatewayMessageContentLimited = 1 << 19,
        ApplicationCommandBadge = 1 << 23,
        ActiveApplication = 1 << 24
    }
}