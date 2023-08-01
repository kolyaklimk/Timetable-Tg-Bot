using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TimetableTgBot.Constants;

namespace TimetableTgBot.TgCommands;

public static class ImageCommands
{
    public static async Task ImageMenu(CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        var MenuMarkup = new InlineKeyboardMarkup(new[]
        {
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("Выбрать диапазон", $"IA{callbackQuery.Message.Date.Month:00}{callbackQuery.Message.Date.Year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("Повторить прошлое изображение", $"\0"),
            },
            PublicConstants.EmptyInlineKeyboardButton,
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu)
            },
        });

        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            "Меню image:",
            replyMarkup: MenuMarkup);
    }
}