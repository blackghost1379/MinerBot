namespace BtcMiner.Helpers
{
    public class ClaimStatus
    {
        public static bool CAN = true;
        public static bool CANT = false;

        public TimeSpan RemainTime { get; set; }
        public bool State { get; set; } = CAN;
    }
}
