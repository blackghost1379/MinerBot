namespace BtcMiner.Entity
{
    public class Task
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Type { get; set; }
        public DateTime? Created { get; set; }
        public required string UserId { get; set; }
        // public User? User { get; set;}
    }
}
