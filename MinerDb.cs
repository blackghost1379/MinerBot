using BtcMiner.Entity;
using BtcMiner.Helpers;
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
                    ProfilePicUrl =
                        "https://api.telegram.org/file/bot6904685786:AAH9dBzy8aONIMGKKVWdhDPLJDllEHXn7R0/photos/file_19.jpg",
                    FirstName = "</Mohammad>",
                    LastName = "",
                    Username = "mohammad_imanni",
                    AllowWritePm = true
                },
                new User
                {
                    Id = 2,
                    TelegramId = "848400596",
                    ProfilePicUrl =
                        "https://api.telegram.org/file/bot6904685786:AAH9dBzy8aONIMGKKVWdhDPLJDllEHXn7R0/photos/file_17.jpg",
                    FirstName = "Mohammad Reza",
                    LastName = "",
                    Username = "MRE01",
                    AllowWritePm = true,
                    IsPremium = true
                }
            );
        modelBuilder
            .Entity<BtcMiner.Entity.Task>()
            .HasData(
                new BtcMiner.Entity.Task
                {
                    Id = 1,
                    Value = "https://x.com/BtcMiner",
                    Name = "Twitter",
                    Type = TaskTypes.JOIN
                },
                new BtcMiner.Entity.Task
                {
                    Id = 2,
                    Value = "https://facebook.com/BtcMiner",
                    Name = "FaceBook",
                    Type = TaskTypes.JOIN
                },
                new BtcMiner.Entity.Task
                {
                    Id = 3,
                    Value = "https://youtube.com/BtcMiner",
                    Name = "youTube",
                    Type = TaskTypes.JOIN
                },
                new BtcMiner.Entity.Task
                {
                    Id = 4,
                    Value = "https://t.me/BtcMiner",
                    Name = "Telegram",
                    Type = TaskTypes.JOIN
                },
                new BtcMiner.Entity.Task
                {
                    Id = 5,
                    Value = "5",
                    Name = "Invite 5 Friend",
                    Type = TaskTypes.INVITE
                },
                new BtcMiner.Entity.Task
                {
                    Id = 6,
                    Value = "10",
                    Name = "Invite 10 Friend",
                    Type = TaskTypes.INVITE
                },
                new BtcMiner.Entity.Task
                {
                    Id = 7,
                    Value = "15",
                    Name = "Invite 15 Friend",
                    Type = TaskTypes.INVITE
                },
                new BtcMiner.Entity.Task
                {
                    Id = 8,
                    Value = "20",
                    Name = "Invite 20 Friend",
                    Type = TaskTypes.INVITE
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
