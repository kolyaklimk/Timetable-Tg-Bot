using Microsoft.EntityFrameworkCore;
using Timetable_Tg_Bot.Enities;

public class BotBdContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<WorkTime> WorkTimes { get; set; }
    public BotBdContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=bot.db");
    }
}