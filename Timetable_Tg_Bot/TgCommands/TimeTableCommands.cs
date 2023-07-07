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
                        row[i] = InlineKeyboardButton.WithCallbackData(currentDay.ToString(), $"CHTT_{currentDay}_{currentDate.Month}_{currentDate.Year}");
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
                currentDate.Year >= 1111 ? InlineKeyboardButton.WithCallbackData("<<",$"CMoTT_{previousMonth.Month}_{previousMonth.Year}") : "\0",
                InlineKeyboardButton.WithCallbackData("\0", "\0"),
                currentDate.Year <= 8888 ? InlineKeyboardButton.WithCallbackData(">>",$"CMoTT_{nextMonth.Month}_{nextMonth.Year}") : "\0", });

            rows.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Меню", Constants.GoMenu), });

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

    public static async Task ChooseHourTimeTable(Match match, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup markup;

        string day = match.Groups[1].Value;
        string month = match.Groups[2].Value;
        string year = match.Groups[3].Value;

        // Hours
        List<InlineKeyboardButton[]> rows = new List<InlineKeyboardButton[]>
        {
            new InlineKeyboardButton[] {
                "\0",
                InlineKeyboardButton.WithCallbackData("22",$"CMiTT_22_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("23",$"CMiTT_23_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("00",$"CMiTT_00_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("01",$"CMiTT_01_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("02",$"CMiTT_02_{day}_{month}_{year}"),
                "\0",
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("20",$"CMiTT_20_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("21",$"CMiTT_21_{day}_{month}_{year}"),
                "\0","\0","\0",
                InlineKeyboardButton.WithCallbackData("03",$"CMiTT_03_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("04",$"CMiTT_04_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("19",$"CMiTT_19_{day}_{month}_{year}"),
                "\0","\0","\0","\0","\0",
                InlineKeyboardButton.WithCallbackData("05",$"CMiTT_05_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("18",$"CMiTT_18_{day}_{month}_{year}"),
                "\0","\0","\0","\0","\0",
                InlineKeyboardButton.WithCallbackData("06",$"CMiTT_06_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("17",$"CMiTT_17_{day}_{month}_{year}"),
                "\0","\0","\0","\0","\0",
                InlineKeyboardButton.WithCallbackData("07",$"CMiTT_07_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("16",$"CMiTT_16_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("15",$"CMiTT_15_{day}_{month}_{year}"),
                "\0","\0","\0",
                InlineKeyboardButton.WithCallbackData("09",$"CMiTT_09_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("08",$"CMiTT_08_{day}_{month}_{year}"),
            },
            new InlineKeyboardButton[] {
                "\0",
                InlineKeyboardButton.WithCallbackData("14",$"CMiTT_14_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("13",$"CMiTT_13_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("12",$"CMiTT_12_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("11",$"CMiTT_11_{day}_{month}_{year}"),
                InlineKeyboardButton.WithCallbackData("10",$"CMiTT_10_{day}_{month}_{year}"),
                "\0",
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("Назад",$"CMoTT_{month}_{year}"),
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
}
