namespace TimetableTgBot;

internal class Program
{
    static void Main(string[] args)
    {
        var botDbContext = new BotDbContext();
        botDbContext.Database.EnsureCreated();

        var bot = new TgBot();
        Console.ReadKey();
    }
}