using BtcMiner.Helpers;

namespace BtcMiner.Entity
{
    public class Transaction
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public int Type { get; set; } = TransactionType.Claim;
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
