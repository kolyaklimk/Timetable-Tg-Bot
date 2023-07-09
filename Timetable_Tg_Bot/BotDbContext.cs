using Microsoft.EntityFrameworkCore;
using TimetableTgBot;
using TimetableTgBot.Entities;

public class BotDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<WorkTime> WorkTimes { get; set; }
    public DbSet<UserState> UserState { get; set; }
    public DbSet<UserBuffer> UserBuffer { get; set; }
    public BotDbContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(PrivateConstants.DB_TOKEN);
    }
}