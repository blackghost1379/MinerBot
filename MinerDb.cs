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
    public DbSet<UserTask> UserTasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //DbSeed(modelBuilder);
    }

    private void DbSeed(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<BtcMiner.Entity.User>()
            .HasData(
                new User
                {
                    Id = 1,
                    TelegramId = "170116448",
                    ProfilePicUrl = "https://googel.com/gb.png",
                    FirstName = "Mohammad",
                    LastName = "Imani",
                    Username = "mohk1379"
                },
                new User
                {
                    Id = 2,
                    TelegramId = "170116441",
                    ProfilePicUrl = "https://googel.com/gb.png",
                    FirstName = "Vahid",
                    LastName = "Aliverdi",
                    Username = "mohk1359"
                }
            );
        modelBuilder
            .Entity<BtcMiner.Entity.Task>()
            .HasData(
                new BtcMiner.Entity.Task
                {
                    Id = 1,
                    Data = "https://x.com/BtcMiner",
                    Name = "Twitter",
                    Type = 0
                },
                new BtcMiner.Entity.Task
                {
                    Id = 2,
                    Data = "https://facebook.com/BtcMiner",
                    Name = "FaceBook",
                    Type = 1
                },
                new BtcMiner.Entity.Task
                {
                    Id = 3,
                    Data = "https://youtube.com/BtcMiner",
                    Name = "youTube",
                    Type = 2
                },
                new BtcMiner.Entity.Task
                {
                    Id = 4,
                    Data = "https://t.me/BtcMiner",
                    Name = "Telegram",
                    Type = 2
                }
            );

        modelBuilder
            .Entity<Referal>()
            .HasData(
                new Referal
                {
                    Id = 1,
                    UserId = 1,
                    ProfilePicUrl = "https://youtube.com/BtcMiner",
                    TelegramId = "170116441",
                    FirstName = "Vahid",
                    LastName = "Aliverdi",
                }
            );
        modelBuilder
            .Entity<UserTask>()
            .HasData(
                new UserTask
                {
                    Id = 1,
                    TaskId = 1,
                    UserId = 1
                },
                new UserTask
                {
                    Id = 2,
                    TaskId = 2,
                    UserId = 1
                },
                new UserTask
                {
                    Id = 3,
                    TaskId = 1,
                    UserId = 2
                },
                new UserTask
                {
                    Id = 4,
                    TaskId = 2,
                    UserId = 2
                }
            );
    }
}
