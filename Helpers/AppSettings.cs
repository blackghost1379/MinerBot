namespace BtcMiner.Helpers
{
    public class AppSettings
    {
        public string? Secret { get; set; }
        public int AddBalance { get; set; }
        public int DefaultClaimCoinsPerHour { get; set; }
        public int DefaultClaimHour { get; set; }
        public int DefaultInviteCoinsPerUser { get; set; }
        public int ClaimTimeInHour { get; set; }
        public int ClaimTimeInSecond { get; set; }
        public int ClaimTimeInMin { get; set; }
        public required string BotToken { get; set; }

    }
}
