namespace BtcMiner.Entity
{
    public class Referal
    {
        public int Id { get; set; }
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public required string UserId { get; set; }
        public required string ProfilePicUrl { get; set; }

        // public User? User { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
