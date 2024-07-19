namespace BtcMiner.Models
{
    public class AuthRequest
    {
        public required string UserId { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }
        public string? ReffererId { get; set; }

        public string? Username { get; set; }

        public string? LanguageCode { get; set; } = "en";

        public required string ProfilePicUrl { get; set; }

        public bool IsPremium { get; set; } = false;

        public bool AllowWritePm { get; set; } = false;
    }
}
