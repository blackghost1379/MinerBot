using BtcMiner.Entity;
using Microsoft.EntityFrameworkCore;

public class MinerDb : DbContext
{
    public MinerDb(DbContextOptions options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<BtcMiner.Entity.Task> Tasks { get; set; }
    public DbSet<Referal> Referals { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
}
