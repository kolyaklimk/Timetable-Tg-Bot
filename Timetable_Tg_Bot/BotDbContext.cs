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
        optionsBuilder.UseSqlite($"Data Source=bot.db");
    }
}