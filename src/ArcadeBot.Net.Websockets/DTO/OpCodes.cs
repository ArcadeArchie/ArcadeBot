namespace ArcadeBot.DTO
{
    ///https://discord.com/developers/docs/topics/opcodes-and-status-codes#gateway-gateway-opcodes
    public static class OpCodes
    {
        public enum Gateway
        {
            Dispatch = 0,
            Heartbeat = 1,
            Identify = 2,
            PresenceUpdate = 3,
            VoiceStateUpdate = 4,
            Resume = 6,
            Reconnect = 7,
            RequestGuildMembers = 8,
            InvalidSession = 9,
            Hello = 10,
            HeartbeatACK = 11
        }
        public enum Voice
        {
            Identify = 0,
            SelectProtocol = 1,
            Ready = 2,
            Heartbeat = 3,
            SessionDescription = 4,
            Speaking = 5,
            HeartbeatACK = 6,
            Resume = 7,
            Hello = 8,
            Resumed = 9,
            ClientDisconnect = 13
        }
    }

    public static class EventCodes
    {
        public readonly static Dictionary<int, (string, bool)> GatewayClose = new()
        {
            [4000] = ("Unknown error", true),
            [4001] = ("Unknown opcode", true),
            [4002] = ("Decode error", true),
            [4003] = ("Not authorized", true),
            [4004] = ("Authentication failed", false),
            [4005] = ("Already authenticated", true),
            [4007] = ("Invalid sequence", true),
            [4008] = ("Rate limited", true),
            [4009] = ("Session timeout", true),
            [4010] = ("Invalid shard", false),
            [4011] = ("Sharding required", false),
            [4012] = ("Invalid API version", false),
            [4013] = ("Invalid intent(s)", false),
            [4014] = ("Disallowed intent(s)", false)
        };
        public readonly static Dictionary<int, string> VoiceClose = new()
        {
            [4001] = "Unknown opcode",
            [4002] = "Falied to decode payload",
            [4003] = "Not authenticated",
            [4004] = "Authentication failed",
            [4005] = "Already Authenticated",
            [4006] = "Session no longer valid",
            [4009] = "Session timeout",
            [4011] = "Server not found",
            [4012] = "Unknown protocol",
            [4014] = "Disconnected",
            [4015] = "Voice server crashed",
            [4016] = "Unknown encryption mode",
        };
    }
}