using Telegram.Bot;
using Telegram.Bot.Types;

namespace TimetableTgBot.TgCommands;

public static class GeneralCommands
{
    public static async Task DeleteMessage(ITelegramBotClient botClient, Message message)
    {
        await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
    }

    public static async Task CreateMenu(bool edit, ITelegramBotClient botClient, Message message)
    {
        if (edit)
        {
            await botClient.EditMessageTextAsync(
                message.Chat.Id,
                message.MessageId,
                "Меню:",
                replyMarkup: Constants.MenuMarkup);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Меню:",
                replyMarkup: Constants.MenuMarkup);
        }
    }
}
