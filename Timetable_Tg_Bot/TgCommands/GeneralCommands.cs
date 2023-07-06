using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TimetableTgBot.TgCommands;

public static class GeneralCommands
{
    public static async Task DeleteMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (message != null)
        {
            try
            {
                await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken);
            }
            catch { }
        }
    }

    public static async Task CreateMenu(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(
            message.Chat.Id,
            "Меню:",
            replyMarkup: Constants.MenuMarkup,
            cancellationToken: cancellationToken);
    }
}
