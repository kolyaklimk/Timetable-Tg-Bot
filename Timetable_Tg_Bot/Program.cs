using System;
using Telegram.Bot;

namespace MyApp 
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var botClient = new TelegramBotClient("");

            Console.ReadKey();
        }
    }
}