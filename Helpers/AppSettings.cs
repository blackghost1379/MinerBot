namespace BtcMiner.Helpers
{
    public class AppSettings
    {
        public string? Secret { get; set; }
        public int AddBalance { get; set; }
        public int InviteAddBalanceCoin { get; set; }
        public int ClaimTimeInHour { get; set; }
        public int ClaimTimeInSecond { get; set; }
        public int ClaimTimeInMin { get; set; }
        public required string BotToken { get; set; }
        public required string ChannelId { get; set; }
    }
}
