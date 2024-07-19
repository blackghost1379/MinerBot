namespace BtcMiner.Helpers
{
    public class ClaimStatus
    {
        public static int CAN = 1;
        public static int CANT = 0;

        public DateTime RemainTime { get; set; }
        public int State { get; set; } = CAN;
    }
}
