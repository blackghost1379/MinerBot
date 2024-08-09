using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace BtcMiner.Entity
{
    [Index(nameof(TelegramId), IsUnique = true)]
    public class User
    {
        public int Id { get; set; }
        public required string TelegramId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public required string ProfilePicUrl { get; set; }
        public string? LanguageCode { get; set; } = "fa";
        public bool IsPremium { get; set; } = false;
        public bool AllowWritePm { get; set; } = false;
        public int Balance { get; set; } = 0;
        public double BtcBalance { get; set; } = 0.0;
        public DateTime Created { get; set; } = DateTime.Now;
        public bool isBan { get; set; } = false;
        public int ClaimCoins { get; set; } = 50000;
        public int ClaimHour { get; set; } = 3;
        public int ClaimRefferal { get; set; } = 15000;

        [JsonIgnore]
        public IList<Referal>? Referals { get; set; }

        [JsonIgnore]
        public IList<Transaction>? Transactions { get; set; }
    }
}
