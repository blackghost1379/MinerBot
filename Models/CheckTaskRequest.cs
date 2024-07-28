namespace BtcMiner.Models
{
    public class CheckTaskRequest
    {
        public required int taskId { get; set; }
        public string? TaskData { get; set; }
    }
}
