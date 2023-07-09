using Microsoft.EntityFrameworkCore;
using TimetableTgBot.Entities;

public class BotDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<WorkTime> WorkTimes => Set<WorkTime>();
    public DbSet<UserState> UserState => Set<UserState>();
    public DbSet<UserBuffer> UserBuffer => Set<UserBuffer>();
    public BotDbContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string projectPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

        string dbFilePath = Path.Combine(projectPath, "Db\\bot.db");

        optionsBuilder.UseSqlite($"Data Source={dbFilePath}");
    }
}