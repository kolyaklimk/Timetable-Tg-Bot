using System.Runtime.InteropServices;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TimetableTgBot.TgCommands;

public static class GeneralCommands
{
    public static async Task DeleteMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken);
        }
        catch
        {
            Console.WriteLine("DeleteMessageAsync error in /start");
        }
    }

    public static async Task CreateMenu(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { Constants.TimeTableMenu, Constants.ImageMenu },
            new KeyboardButton[] { Constants.SupportMenu, Constants.SubscribeMenu }
        }); 
        
        var mes = await botClient.SendTextMessageAsync(
            message.Chat.Id,
            "Меню:",
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
    }
}
