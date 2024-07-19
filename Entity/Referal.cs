using Microsoft.EntityFrameworkCore;

namespace BtcMiner.Entity
{
    [Index(nameof(UserId), IsUnique = true)]
    public class Referal
    {
        public int Id { get; set; }
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public required string TelegramId { get; set; }
        public required string ProfilePicUrl { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
