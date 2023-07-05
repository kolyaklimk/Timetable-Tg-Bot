using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TimetableTgBot.TgCommands;

public static class GeneralCommands
{
    public static async Task DeleteMessage(ITelegramBotClient botClient, Message message)
    {
        try
        {
            await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
        }
        catch
        {
            Console.WriteLine("DeleteMessageAsync error in /start");
        }
    }
}
