using Microsoft.EntityFrameworkCore;

namespace BtcMiner.Entity
{
    [PrimaryKey(nameof(Id))]
    public class Task
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Type { get; set; }
        public string? Data { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
