using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace BtcMiner.Entity
{
    [PrimaryKey(nameof(Id))]
    public class Task
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Type { get; set; }
        public string? Value { get; set; }
        public int Balance { get; set; } = 0;
        [JsonIgnore]
        public string? Code { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
