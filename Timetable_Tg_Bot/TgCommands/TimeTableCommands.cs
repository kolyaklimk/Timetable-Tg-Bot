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

    public static async Task MenuTimeTable(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        // Send message
        await botClient.EditMessageTextAsync(
            message.Chat.Id,
            message.MessageId,
            $"Меню расписания:",
            replyMarkup: Constants.TimeTableMenuMarkup,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);
    }

    public static async Task ChooseDateTimeTable(Match match, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
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
                        row[i] = InlineKeyboardButton.WithCallbackData(currentDay.ToString(), $"TMD_{currentDay}_{currentDate.Month}_{currentDate.Year}");
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
                currentDate.Year >= 1111 ? InlineKeyboardButton.WithCallbackData("<<",$"TCMo_{previousMonth.Month}_{previousMonth.Year}") : "\0",
                Constants.Empty,
                currentDate.Year <= 8888 ? InlineKeyboardButton.WithCallbackData(">>",$"TCMo_{nextMonth.Month}_{nextMonth.Year}") : "\0", });
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
            message.Chat.Id,
            message.MessageId,
            "Выберите дату:",
            replyMarkup: markup,
            cancellationToken: cancellationToken);
    }

    public static async Task MenuDayTimeTable(Match match, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        string day = match.Groups[1].Value;
        string month = match.Groups[2].Value;
        string year = match.Groups[3].Value;

        List<InlineKeyboardButton[]> rows = new List<InlineKeyboardButton[]> {
            new InlineKeyboardButton[]{
                InlineKeyboardButton.WithCallbackData("Удалить время","\0"),
                InlineKeyboardButton.WithCallbackData("Выбрать время", $"TCH_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[]{
                InlineKeyboardButton.WithCallbackData("Удалить всё","\0"),
                InlineKeyboardButton.WithCallbackData("Выбрать шаблон", "\0"),
            },
            Constants.EmptyInlineKeyboardButton,
            new InlineKeyboardButton[]{
                InlineKeyboardButton.WithCallbackData("Назад", $"TCMo_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("Меню", Constants.GoMenu),
            }
        };

        // Send message
        await botClient.EditMessageTextAsync(
            message.Chat.Id,
            message.MessageId,
            $"Вы выбрали: __*{day}/{month}/{year}*__\nВыберите час:",
            replyMarkup: new InlineKeyboardMarkup(rows),
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);
    }

    public static async Task ChooseHourTimeTable(Match match, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        string day = match.Groups[1].Value;
        string month = match.Groups[2].Value;
        string year = match.Groups[3].Value;

        // Hours
        List<InlineKeyboardButton[]> rows = new List<InlineKeyboardButton[]>
        {
            new InlineKeyboardButton[] {
                "\0",
                InlineKeyboardButton.WithCallbackData("22",$"TCMi_22_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("23",$"TCMi_23_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("00",$"TCMi_00_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("01",$"TCMi_01_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("02",$"TCMi_02_{day}_{month}_{year}"),
                "\0",
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("20",$"TCMi_20_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("21",$"TCMi_21_{day}_{month}_{year}"),
                "\0","\0","\0",
                InlineKeyboardButton.WithCallbackData("03",$"TCMi_03_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("04",$"TCMi_04_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("19",$"TCMi_19_{day}_{month}_{year}"),
                "\0","\0","\0","\0","\0",
                InlineKeyboardButton.WithCallbackData("05",$"TCMi_05_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("18",$"TCMi_18_{day}_{month}_{year}"),
                "\0","\0",Constants.Empty,"\0","\0",
                InlineKeyboardButton.WithCallbackData("06",$"TCMi_06_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("17",$"TCMi_17_{day}_{month}_{year}"),
                "\0","\0","\0","\0","\0",
                InlineKeyboardButton.WithCallbackData("07",$"TCMi_07_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("16",$"TCMi_16_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("15",$"TCMi_15_{day}_{month}_{year}"),
                "\0","\0","\0",
                InlineKeyboardButton.WithCallbackData("09",$"TCMi_09_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("08",$"TCMi_08_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                "\0",
                InlineKeyboardButton.WithCallbackData("14",$"TCMi_14_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("13",$"TCMi_13_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("12",$"TCMi_12_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("11",$"TCMi_11_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("10",$"TCMi_10_{day}_{month}_{year}"),
                "\0",
            },
            new InlineKeyboardButton[] { "\0" },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("Назад",$"TMD_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("Меню", Constants.GoMenu),
            }
        };

        // Send message
        await botClient.EditMessageTextAsync(
            message.Chat.Id,
            message.MessageId,
            $"Вы выбрали: __*{day}/{month}/{year}*__\nВыберите час:",
            replyMarkup: new InlineKeyboardMarkup(rows),
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);
    }

    public static async Task ChooseMinuteTimeTable(Match match, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        string hour = match.Groups[1].Value;
        string day = match.Groups[2].Value;
        string month = match.Groups[3].Value;
        string year = match.Groups[4].Value;

        // Hours
        List<InlineKeyboardButton[]> rows = new List<InlineKeyboardButton[]>
        {
            new InlineKeyboardButton[] {
                "\0",
                InlineKeyboardButton.WithCallbackData("55",$"TCMi_55_{hour}_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("00",$"TCMi_00_{hour}_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("05",$"TCMi_05_{hour}_{day}_{month}_{year}"),
                "\0",
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("50",$"TCMi_50_{hour}_{day}_{month}_{year}"),
                "\0","\0","\0",
                InlineKeyboardButton.WithCallbackData("10",$"TCMi_10_{hour}_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("45",$"TCMi_45_{hour}_{day}_{month}_{year}"),
                "\0",Constants.Empty,"\0",
                InlineKeyboardButton.WithCallbackData("15",$"TCMi_15_{hour}_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("40",$"TCMi_40_{hour}_{day}_{month}_{year}"),
                "\0","\0","\0",
                InlineKeyboardButton.WithCallbackData("20",$"TCMi_20_{hour}_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                "\0",
                InlineKeyboardButton.WithCallbackData("35",$"TCMi_55_{hour}_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("30",$"TCMi_30_{hour}_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("25",$"TCMi_25_{hour}_{day}_{month}_{year}"),
                "\0",
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("Назад",$"TCH_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("Меню", Constants.GoMenu),
            }
        };

        // Send message
        await botClient.EditMessageTextAsync(
            message.Chat.Id,
            message.MessageId,
            "Вы выбрали:\n" +
            $"Дата: __*{day}/{month}/{year}*__\n" +
            $"Час: __*{hour}*__",
            replyMarkup: new InlineKeyboardMarkup(rows),
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);
    }
}
