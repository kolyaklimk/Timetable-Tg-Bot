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

    public static async Task ChooseMonth(CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, PublicConstants.ChooseMonthImage);
        string year = match.Groups[1].Value;

        // Months in year
        var rows = new List<InlineKeyboardButton[]>
        {
            new[] { InlineKeyboardButton.WithCallbackData($"{year}", "\0") }
        };
        for (var i = 1; i <= 12;)
        {
            var row = new InlineKeyboardButton[3];
            for (var j = 0; j < 3; j++)
            {
                row[j] = InlineKeyboardButton.WithCallbackData(PublicConstants.Months[i - 1], $"IA{i:00}{year}MM");
                i++;
            }
            rows.Add(row);
        }

        rows.Add(new[] {
            int.Parse(year) >=  callbackQuery.Message.Date.AddYears(-1).Year ? InlineKeyboardButton.WithCallbackData("<<", $"IC{int.Parse(year) - 1}") : "\0",
            PublicConstants.EmptyInlineKeyboardButton[0],
            int.Parse(year) <= callbackQuery.Message.Date.AddYears(1).Year ? InlineKeyboardButton.WithCallbackData(">>",$"IC{int.Parse(year) + 1}") : "\0",
        });

        rows.Add(new[] {
            InlineKeyboardButton.WithCallbackData("Назад", $"IM"),
            InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu),
        });

        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            "Выбрать откуда начинать:",
            replyMarkup: new InlineKeyboardMarkup(rows));
    }
}