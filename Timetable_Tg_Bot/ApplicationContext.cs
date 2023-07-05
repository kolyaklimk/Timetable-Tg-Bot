using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;

public class ApplicationContext : DbContext
{
    public ApplicationContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=users.db");
    }
}