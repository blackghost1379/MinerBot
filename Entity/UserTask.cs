using Microsoft.EntityFrameworkCore;

namespace BtcMiner.Entity
{
    [PrimaryKey(nameof(Id))]
    public class UserTask
    {
        public int Id { get; set; }
        public User? User { get; set; }
        public int UserId { get; set; }
        public Task? Task { get; set; }
        public int TaskId { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
