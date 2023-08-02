using Microsoft.EntityFrameworkCore;
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

    public static async Task RangeOfDaysImage(BotDbContext context, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, PublicConstants.RangeOfDaysImage);
        char property = match.Groups[1].Value[0];

        var userBuffer = await context.UserBuffers.FirstOrDefaultAsync(arg => arg.UserId == callbackQuery.From.Id);
        switch (property)
        {
            // Save to Buffer
            case '1':
                var date1 = DateOnly.Parse($"{match.Groups[2].Value}/{match.Groups[3].Value}/{match.Groups[4].Value}");
                var date2 = DateOnly.Parse($"{match.Groups[5].Value}/{match.Groups[6].Value}/{match.Groups[7].Value}");

                if (Math.Abs(date1.DayNumber - date2.DayNumber) > 40)
                {
                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Нельзя выбрать больше 40 дней", true);
                    return;
                }

                userBuffer.ImageDays = new()
                {
                    date1 < date2 ? date1 : date2,
                    date1 > date2 ? date1 : date2
                };
                break;

            // Add deleted date
            case '2':
                userBuffer.ImageDays.Add(DateOnly.Parse($"{match.Groups[2].Value}/{match.Groups[3].Value}/{match.Groups[4].Value}"));
                break;

            // Remove deleted date
            case '3':
                var index = userBuffer.ImageDays.FindLastIndex(arg => arg == DateOnly.Parse($"{match.Groups[2].Value}/{match.Groups[3].Value}/{match.Groups[4].Value}"));
                userBuffer.ImageDays.RemoveAt(index);
                break;
        }
        await context.SaveChangesAsync();

        var rows = new List<InlineKeyboardButton[]> { PublicConstants.WeekButtons };
        var dayCount = userBuffer.ImageDays[1].DayNumber - userBuffer.ImageDays[0].DayNumber;

        HashSet<DateOnly> deletedDays = new();
        for (var i = 2; i < userBuffer.ImageDays.Count; i++)
            deletedDays.Add(userBuffer.ImageDays[i]);

        // Days
        var currentDay = userBuffer.ImageDays[0];
        var currentMonth = currentDay.Month - 1;
        int firstDayOfMonth = 0;
        int rowCount = 0;
        while (currentDay <= userBuffer.ImageDays[1])
        {
            if (currentDay.Month != currentMonth)
            {
                currentMonth = currentDay.Month;
                firstDayOfMonth = ((int)currentDay.DayOfWeek + 6) % 7;
                rowCount = rows.Count + 1;
                rows.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(PublicConstants.Months[currentMonth], "\0") });
            }

            var row = new InlineKeyboardButton[7];
            for (int i = 0; i < 7; i++)
            {
                if (currentDay <= userBuffer.ImageDays[1] && (i >= firstDayOfMonth || rows.Count > rowCount) && currentDay.Month == currentMonth)
                {
                    if (!deletedDays.Contains(currentDay))
                    {
                        row[i] = InlineKeyboardButton.WithCallbackData(currentDay.Day.ToString(), $"IR2{currentDay.Day:00}{currentDay.Month:00}{currentDay.Year}");
                    }
                    else
                    {
                        row[i] = InlineKeyboardButton.WithCallbackData(PublicConstants.CrossLittleNumbers[currentDay.Day], $"IR3{currentDay.Day:00}{currentDay.Month:00}{currentDay.Year}");
                    }
                    currentDay = currentDay.AddDays(1);
                }
                else
                {
                    row[i] = "\0";
                }
            }
            rows.Add(row);
        }

        rows.Add(new[] { InlineKeyboardButton.WithCallbackData("Продолжить", $"IH") });
        rows.Add(PublicConstants.EmptyInlineKeyboardButton);
        rows.Add(new[] {
            InlineKeyboardButton.WithCallbackData("Назад", $"IB{userBuffer.ImageDays[1].Month:00}{userBuffer.ImageDays[1].Year}{userBuffer.ImageDays[0].Day:00}{userBuffer.ImageDays[0].Month:00}{userBuffer.ImageDays[0].Year}"),
            InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu),
        });

        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            "Выбрать откуда начинать:",
            replyMarkup: new InlineKeyboardMarkup(rows));
    }
}