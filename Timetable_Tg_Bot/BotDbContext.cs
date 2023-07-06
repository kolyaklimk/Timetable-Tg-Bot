using Microsoft.EntityFrameworkCore;
using TimetableTgBot.Entities;

public class BotDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<WorkTime> WorkTimes => Set<WorkTime>();
    public BotDbContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string projectPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

        string dbFilePath = Path.Combine(projectPath, "bot.db");

        optionsBuilder.UseSqlite($"Data Source={dbFilePath}");
    }
}