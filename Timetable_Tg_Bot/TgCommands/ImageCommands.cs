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
                InlineKeyboardButton.WithCallbackData("1", $"IA{callbackQuery.Message.Date.Month:00}{callbackQuery.Message.Date.Year}01"),
                InlineKeyboardButton.WithCallbackData("2", $"IA{callbackQuery.Message.Date.Month:00}{callbackQuery.Message.Date.Year}02"),
                InlineKeyboardButton.WithCallbackData("3", $"IA{callbackQuery.Message.Date.Month:00}{callbackQuery.Message.Date.Year}03"),
                InlineKeyboardButton.WithCallbackData("4", $"IA{callbackQuery.Message.Date.Month:00}{callbackQuery.Message.Date.Year}04"),
                InlineKeyboardButton.WithCallbackData("5", $"IA{callbackQuery.Message.Date.Month:00}{callbackQuery.Message.Date.Year}05"),
                InlineKeyboardButton.WithCallbackData("6", $"IA{callbackQuery.Message.Date.Month:00}{callbackQuery.Message.Date.Year}06"),
                InlineKeyboardButton.WithCallbackData("7", $"IA{callbackQuery.Message.Date.Month:00}{callbackQuery.Message.Date.Year}07")
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("2 недели", $"IA{callbackQuery.Message.Date.Month:00}{callbackQuery.Message.Date.Year}14"),
                InlineKeyboardButton.WithCallbackData("3 недели", $"IA{callbackQuery.Message.Date.Month:00}{callbackQuery.Message.Date.Year}21"),
                InlineKeyboardButton.WithCallbackData("4 недели", $"IA{callbackQuery.Message.Date.Month:00}{callbackQuery.Message.Date.Year}28")
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("Определённый месяц", $"IC{callbackQuery.Message.Date.Year}")
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