namespace BtcMiner.Entity
{
    public class User
    {
        public int Id { get; set; }
        public required string UserId { get; set; }

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
    }
}
