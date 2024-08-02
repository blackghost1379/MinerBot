using BtcMiner.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BtcMiner.Entity
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int Type { get; set; } = TransactionType.Claim;
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
