using Microsoft.EntityFrameworkCore;
using Timetable_Tg_Bot.Enities;

public class BotDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<WorkTime> WorkTimes => Set<WorkTime>();
    public BotDbContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=bot.db");
    }
}