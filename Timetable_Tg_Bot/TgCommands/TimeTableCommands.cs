using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TimetableTgBot.TgCommands;

public static class TimeTableCommands
{
    private static Dictionary<string, InlineKeyboardMarkup> SavedCalendars = new();

    public static async Task MenuTimeTable(ITelegramBotClient botClient, Message message)
    {
        // Send message
        await botClient.EditMessageTextAsync(
            message.Chat.Id,
            message.MessageId,
            $"Меню расписания:",
            replyMarkup: Constants.TimeTableMenuMarkup,
            parseMode: ParseMode.MarkdownV2);
    }

    public static async Task ChooseDateTimeTable(CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery.Data, Constants.ChooseMonthTimeTable);

        InlineKeyboardMarkup markup;

        if (SavedCalendars.ContainsKey($"{match.Groups[1].Value}_{match.Groups[2].Value}"))
        {
            markup = SavedCalendars[$"{match.Groups[1].Value}_{match.Groups[2].Value}"];
        }
        else
        {
            #region Create calendar
            DateTime currentDate = DateTime.Parse($"1/{match.Groups[1].Value}/{match.Groups[2].Value}");
            int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
            int firstDayOfMonth = ((int)currentDate.DayOfWeek + 6) % 7;
            var monthName = currentDate.ToString("MMMM", new CultureInfo("ru-RU"));

            // Month and Name day of week
            List<InlineKeyboardButton[]> rows = new List<InlineKeyboardButton[]>
            {
                new InlineKeyboardButton[] {
                    InlineKeyboardButton.WithCallbackData($"{char.ToUpper(monthName[0])}{monthName.Substring(1)} {currentDate.Year}", "\0")},
                Constants.WeekButtons
            };

            // Calendar
            int currentDay = 1;
            while (currentDay <= daysInMonth)
            {
                InlineKeyboardButton[] row = new InlineKeyboardButton[7];

                for (int i = 0; i < 7; i++)
                {
                    if (currentDay <= daysInMonth && (i >= firstDayOfMonth || rows.Count > 2))
                    {
                        row[i] = InlineKeyboardButton.WithCallbackData(currentDay.ToString(), $"TG_{currentDay}_{currentDate.Month}_{currentDate.Year}");
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

            rows.Add(new InlineKeyboardButton[] {
                currentDate.Year >=  DateTime.Now.AddYears(-1).Year ? InlineKeyboardButton.WithCallbackData("<<",$"TA_{previousMonth.Month}_{previousMonth.Year}") : "\0",
                Constants.Empty,
                currentDate.Year <= DateTime.Now.AddYears(1).Year ? InlineKeyboardButton.WithCallbackData(">>",$"TA_{nextMonth.Month}_{nextMonth.Year}") : "\0", });
            rows.Add(new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("Назад", Constants.MenuTimeTable),
                InlineKeyboardButton.WithCallbackData("Меню", Constants.GoMenu),
            });

            markup = new InlineKeyboardMarkup(rows);
            SavedCalendars.Add($"{currentDate.Month}_{currentDate.Year}", markup);
            #endregion
        }

        // Send message
        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            "Выберите дату:",
            replyMarkup: markup);
    }

    public static async Task MenuDayTimeTable(CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery.Data, Constants.MenuDayTimeTable);

        string day = match.Groups[1].Value;
        string month = match.Groups[2].Value;
        string year = match.Groups[3].Value;

        DateTime currentDate = DateTime.Parse($"{day}/{month}/{year}");

        // previous and next buttons
        var previousMonth = currentDate.AddDays(-1);
        var nextMonth = currentDate.AddDays(1);

        List<InlineKeyboardButton[]> rows = new List<InlineKeyboardButton[]> {
            new InlineKeyboardButton[]{
                InlineKeyboardButton.WithCallbackData("Удалить время","\0"),
                InlineKeyboardButton.WithCallbackData("Выбрать время", $"TB_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[]{
                InlineKeyboardButton.WithCallbackData("Удалить всё","\0"),
                InlineKeyboardButton.WithCallbackData("Выбрать шаблон", "\0"),
            },
            new InlineKeyboardButton[]{
                currentDate.Year >=  DateTime.Now.AddYears(-1).Year ? InlineKeyboardButton.WithCallbackData("<<",$"TG_{previousMonth.Day}_{previousMonth.Month}_{previousMonth.Year}") : "\0",
                Constants.Empty,
                currentDate.Year <=  DateTime.Now.AddYears(1).Year ? InlineKeyboardButton.WithCallbackData(">>",$"TG_{nextMonth.Day}_{nextMonth.Month}_{nextMonth.Year}") : "\0",
            },
            new InlineKeyboardButton[]{
                InlineKeyboardButton.WithCallbackData("Назад", $"TA_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("Меню", Constants.GoMenu),
            }
        };

        // Send message
        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            $"Вы выбрали: __*{day}/{month}/{year}*__\nВыберите час:",
            replyMarkup: new InlineKeyboardMarkup(rows),
            parseMode: ParseMode.MarkdownV2);
    }

    public static async Task ChooseHourTimeTable(CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, Constants.ChooseHourTimeTable);

        string day = match.Groups[1].Value;
        string month = match.Groups[2].Value;
        string year = match.Groups[3].Value;

        // Hours
        List<InlineKeyboardButton[]> rows = new List<InlineKeyboardButton[]>
        {
            new InlineKeyboardButton[] {
                "\0",
                InlineKeyboardButton.WithCallbackData("22",$"TC_22_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("23",$"TC_23_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("00",$"TC_00_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("01",$"TC_01_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("02",$"TC_02_{day}_{month}_{year}"),
                "\0",
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("20",$"TC_20_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("21",$"TC_21_{day}_{month}_{year}"),
                "\0","\0","\0",
                InlineKeyboardButton.WithCallbackData("03",$"TC_03_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("04",$"TC_04_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("19",$"TC_19_{day}_{month}_{year}"),
                "\0","\0","\0","\0","\0",
                InlineKeyboardButton.WithCallbackData("05",$"TC_05_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("18",$"TC_18_{day}_{month}_{year}"),
                "\0","\0",Constants.Empty,"\0","\0",
                InlineKeyboardButton.WithCallbackData("06",$"TC_06_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("17",$"TC_17_{day}_{month}_{year}"),
                "\0","\0","\0","\0","\0",
                InlineKeyboardButton.WithCallbackData("07",$"TC_07_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("16",$"TC_16_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("15",$"TC_15_{day}_{month}_{year}"),
                "\0","\0","\0",
                InlineKeyboardButton.WithCallbackData("09",$"TC_09_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("08",$"TC_08_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                "\0",
                InlineKeyboardButton.WithCallbackData("14",$"TC_14_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("13",$"TC_13_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("12",$"TC_12_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("11",$"TC_11_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("10",$"TC_10_{day}_{month}_{year}"),
                "\0",
            },
            new InlineKeyboardButton[] { "\0" },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("Назад",$"TG_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("Меню", Constants.GoMenu),
            }
        };

        // Send message
        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            $"Вы выбрали: __*{day}/{month}/{year}*__\nВыберите час:",
            replyMarkup: new InlineKeyboardMarkup(rows),
            parseMode: ParseMode.MarkdownV2);
    }

    public static async Task ChooseMinuteTimeTable(CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, Constants.ChooseMinuteTimeTable);

        string hour = match.Groups[1].Value;
        string day = match.Groups[2].Value;
        string month = match.Groups[3].Value;
        string year = match.Groups[4].Value;

        // Hours
        List<InlineKeyboardButton[]> rows = new List<InlineKeyboardButton[]>
        {
            new InlineKeyboardButton[] {
                "\0",
                InlineKeyboardButton.WithCallbackData("55",$"TD_55_{hour}_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("00",$"TD_00_{hour}_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("05",$"TD_05_{hour}_{day}_{month}_{year}"),
                "\0",
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("50",$"TD_50_{hour}_{day}_{month}_{year}"),
                "\0","\0","\0",
                InlineKeyboardButton.WithCallbackData("10",$"TD_10_{hour}_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("45",$"TD_45_{hour}_{day}_{month}_{year}"),
                "\0",Constants.Empty,"\0",
                InlineKeyboardButton.WithCallbackData("15",$"TD_15_{hour}_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("40",$"TD_40_{hour}_{day}_{month}_{year}"),
                "\0","\0","\0",
                InlineKeyboardButton.WithCallbackData("20",$"TD_20_{hour}_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                "\0",
                InlineKeyboardButton.WithCallbackData("35",$"TD_55_{hour}_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("30",$"TD_30_{hour}_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("25",$"TD_25_{hour}_{day}_{month}_{year}"),
                "\0",
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("Назад",$"TB_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("Меню", Constants.GoMenu),
            }
        };

        // Send message
        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            "Вы выбрали:\n" +
            $"Дата: __*{day}/{month}/{year}*__\n" +
            $"Час: __*{hour}*__",
            replyMarkup: new InlineKeyboardMarkup(rows),
            parseMode: ParseMode.MarkdownV2);
    }

    public static async Task ChooseIsBusyTimeTable(CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, Constants.ChooseIsBusyTimeTable);

        string minute = match.Groups[1].Value;
        string hour = match.Groups[2].Value;
        string day = match.Groups[3].Value;
        string month = match.Groups[4].Value;
        string year = match.Groups[5].Value;

        List<InlineKeyboardButton[]> rows = new List<InlineKeyboardButton[]> {
            new InlineKeyboardButton[]{
                InlineKeyboardButton.WithCallbackData("Свободно",$"TE_0_{minute}_{hour}_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("Запись", $"TE_1_{minute}_{hour}_{day}_{month}_{year}"),
            },
            Constants.EmptyInlineKeyboardButton,
            new InlineKeyboardButton[]{
                InlineKeyboardButton.WithCallbackData("Назад", $"TC_{hour}_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("Меню", Constants.GoMenu),
            }
        };

        // Send message
        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            $"Вы свободны?\n" +
            $"Дата: {day}/{month}/{year}\n" +
            $"Время: {hour}:{minute}",
            replyMarkup: new InlineKeyboardMarkup(rows),
            parseMode: ParseMode.MarkdownV2);
    }

    public static async Task<int> AddDescriptionTimeTable(string? descrption, string data, ITelegramBotClient botClient, Chat chat, int messageId)
    {
        Match match = Regex.Match(data, Constants.AddDescriptionTimeTable);

        string isBusy = match.Groups[1].Value;
        string minute = match.Groups[2].Value;
        string hour = match.Groups[3].Value;
        string day = match.Groups[4].Value;
        string month = match.Groups[5].Value;
        string year = match.Groups[6].Value;

        List<InlineKeyboardButton[]> rows = new List<InlineKeyboardButton[]> {
            new InlineKeyboardButton[]{
                InlineKeyboardButton.WithCallbackData(
                    descrption != null ? $"Сохранить" : "Сохранить без изменений",$"TF_{isBusy}_{minute}_{hour}_{day}_{month}_{year}_"),
            },
            Constants.EmptyInlineKeyboardButton,
            new InlineKeyboardButton[]{
                InlineKeyboardButton.WithCallbackData("Назад", $"TD_{minute}_{hour}_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("Меню", Constants.GoMenu),
            }
        };

        // Send message
        var messageBot = await botClient.EditMessageTextAsync(
            chat.Id,
            messageId,
            $"Напишите, если хотите изменить описание к этой записи :\\)" +
            $"Дата: {day}/{month}/{year}\n" +
            $"Время: {hour}:{minute}\n" +
            $"Запись: {"1" == isBusy}" + (descrption != null ? $"\nОписание: {descrption}" : ""),
            replyMarkup: new InlineKeyboardMarkup(rows),
            parseMode: ParseMode.MarkdownV2);

        return messageBot.MessageId;
    }

    public static async Task SaveTimeTable(BotDbContext dbContext, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, Constants.SaveTimeTable);

        string isBusy = match.Groups[1].Value;
        string minute = match.Groups[2].Value;
        string hour = match.Groups[3].Value;
        string day = match.Groups[4].Value;
        string month = match.Groups[5].Value;
        string year = match.Groups[6].Value;
        string description = match.Groups[7].Value;

        await dbContext.WorkTimes.AddAsync(new Entities.WorkTime
        {
            Date = DateOnly.Parse($"{day}/{month}/{year}"),
            Start = TimeOnly.Parse($"{hour}:{minute}"),
            IsBusy = "1" == isBusy,
            UserId = callbackQuery.From.Id,
            Description = description
        });

        await dbContext.SaveChangesAsync();

        // сохранить в бд поменять везде на строки Match и вызвать MenuDay
        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"Запись на {day}/{month}/{year} в {hour}:{minute} сохранена!", false);
    }
}
