using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TimetableTgBot.Constants;

namespace TimetableTgBot.TgCommands;

public static class GeneralCommands
{
    public static async Task DeleteMessage(ITelegramBotClient botClient, Message message, bool previous = false)
    {
        if (previous)
        {
            while (true)
            {
                try
                {
                    await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId--);
                }
                catch
                {
                    break;
                }
            }
        }
        else
        {
            await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
        }
    }

    public static async Task CreateMenu(bool edit, ITelegramBotClient botClient, Message message)
    {
        var MenuMarkup = new InlineKeyboardMarkup(new[]
        {
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("Изображение", PublicConstants.ImageMenu),
                InlineKeyboardButton.WithCallbackData("Расписание", $"TA{message.Date.Month:00}{message.Date.Year}") },
            new InlineKeyboardButton[] {
                PublicConstants.SupportMenu,
                PublicConstants.SubscribeMenu },
            PublicConstants.EmptyInlineKeyboardButton,
        });

        if (edit)
        {
            await botClient.EditMessageTextAsync(
                message.Chat.Id,
                message.MessageId,
                "Меню:",
                replyMarkup: MenuMarkup);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Меню:",
                replyMarkup: MenuMarkup);
        }
    }
}
