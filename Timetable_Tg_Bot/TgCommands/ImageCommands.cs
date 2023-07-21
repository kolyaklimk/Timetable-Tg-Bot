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
                InlineKeyboardButton.WithCallbackData("1", "IR01"),
                InlineKeyboardButton.WithCallbackData("2", "IR02"),
                InlineKeyboardButton.WithCallbackData("3", "IR03"),
                InlineKeyboardButton.WithCallbackData("4", "IR04"),
                InlineKeyboardButton.WithCallbackData("5", "IR05"),
                InlineKeyboardButton.WithCallbackData("6", "IR06"),
                InlineKeyboardButton.WithCallbackData("7", "IR07")
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("2 недели", "IR14"),
                InlineKeyboardButton.WithCallbackData("3 недели", "IR21"),
                InlineKeyboardButton.WithCallbackData("4 недели", "IR28")
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