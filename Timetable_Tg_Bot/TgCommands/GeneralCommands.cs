using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
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

    public static async Task ChooseDay(string next, string? previous, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery.Data, PublicConstants.ChooseDay);

        string property = match.Groups[1].Value;
        string month = match.Groups[2].Value;
        string year = match.Groups[3].Value;
        string otherInfo = match.Groups[4].Value;

        DateOnly currentDate = DateOnly.ParseExact($"01/{month}/{year}", PublicConstants.DateFormat, null);
        int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
        int firstDayOfMonth = ((int)currentDate.DayOfWeek + 6) % 7;

        // Month and Name day of week
        var rows = new List<InlineKeyboardButton[]>
        {
            new[] { InlineKeyboardButton.WithCallbackData($"{PublicConstants.Months[currentDate.Month]} {currentDate.Year}", "\0") },
            PublicConstants.WeekButtons
        };

        // Calendar
        int currentDay = 1;
        while (currentDay <= daysInMonth)
        {
            var row = new InlineKeyboardButton[7];

            for (int i = 0; i < 7; i++)
            {
                if (currentDay <= daysInMonth && (i >= firstDayOfMonth || rows.Count > 2))
                {
                    row[i] = InlineKeyboardButton.WithCallbackData(currentDay.ToString(), $"{next}{currentDay:00}{month}{year}{otherInfo}");
                    currentDay++;
                }
                else
                {
                    row[i] = "\0";
                }
            }
            rows.Add(row);
        }

        // previous and next buttons
        var previousMonth = currentDate.AddMonths(-1);
        var nextMonth = currentDate.AddMonths(1);

        rows.Add(new[] {
            currentDate.Year >=  callbackQuery.Message.Date.AddYears(-1).Year ? InlineKeyboardButton.WithCallbackData("<<",$"{property}{previousMonth.Month:00}{previousMonth.Year}{otherInfo}") : "\0",
            PublicConstants.EmptyInlineKeyboardButton[0],
            currentDate.Year <= callbackQuery.Message.Date.AddYears(1).Year ? InlineKeyboardButton.WithCallbackData(">>",$"{property}{nextMonth.Month:00}{nextMonth.Year}{otherInfo}") : "\0",
        });
        if (previous == null)
        {
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu), });
        }
        else
        {
            rows.Add(new[] {
                    InlineKeyboardButton.WithCallbackData("Назад", $"{previous}"),
                    InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu),
                });
        }

        // Send message
        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            "Выберите дату:",
            replyMarkup: new InlineKeyboardMarkup(rows),
            parseMode: ParseMode.MarkdownV2);
    }
}
