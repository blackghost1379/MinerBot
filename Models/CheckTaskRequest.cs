namespace BtcMiner.Models
{
    public class CheckTaskRequest
    {
        public required int TaskId { get; set; }
        public string? TaskData { get; set; }
    }
}
