namespace BtcMiner.Models
{
    public class AuthResponse
    {
        public int StatusCode { get; set; } = StatusCodes.Status200OK;
        public required dynamic Data { get; set; }
        public string? Message { get; set; }
    }
}
