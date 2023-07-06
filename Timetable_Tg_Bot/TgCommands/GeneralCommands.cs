using Telegram.Bot;
using Telegram.Bot.Types;

namespace TimetableTgBot.TgCommands;

public static class GeneralCommands
{
    public static async Task DeleteMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken);
    }

    public static async Task CreateMenu(bool edit, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (edit)
        {
            await botClient.EditMessageTextAsync(
                message.Chat.Id,
                message.MessageId,
                "Меню:",
                replyMarkup: Constants.MenuMarkup,
                cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Меню:",
                replyMarkup: Constants.MenuMarkup,
                cancellationToken: cancellationToken);
        }
    }
}
