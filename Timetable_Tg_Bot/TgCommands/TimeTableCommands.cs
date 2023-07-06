using System.Globalization;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TimetableTgBot.TgCommands;

public static class TimeTableCommands
{
    private static Dictionary<string, InlineKeyboardMarkup> SavedCalendars = new();

    public static async Task ChooseDateTimeTable(Match match, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup markup;

        if (SavedCalendars.ContainsKey(match.Groups[1].Value + "_" + match.Groups[2].Value))
        {
            markup = SavedCalendars[match.Groups[1].Value + "_" + match.Groups[2].Value];
        }
        else
        {
            #region Create calendar
            DateTime currentDate = DateTime.Parse("1/" + match.Groups[1].Value + "/" + match.Groups[2].Value);
            int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
            int firstDayOfMonth = ((int)currentDate.DayOfWeek + 6) % 7;
            var monthName = currentDate.ToString("MMMM", new CultureInfo("ru-RU"));

            // Month and Name day of week
            List<InlineKeyboardButton[]> rows = new List<InlineKeyboardButton[]>
            {
                new InlineKeyboardButton[] {
                    InlineKeyboardButton.WithCallbackData(char.ToUpper(monthName[0]) + monthName.Substring(1) + ' ' + currentDate.Year, "\0")},
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
                        row[i] = InlineKeyboardButton.WithCallbackData(currentDay.ToString(), $"choose_date_{currentDay}_{currentDate.Month}_{currentDate.Year}");
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
                currentDate.Year >= 1111 ? InlineKeyboardButton.WithCallbackData("<<",$"ChooseMonthTimeTable_{previousMonth.Month}_{previousMonth.Year}") : "\0",
                InlineKeyboardButton.WithCallbackData("\0", "\0"),
                currentDate.Year <= 8888 ? InlineKeyboardButton.WithCallbackData(">>",$"ChooseMonthTimeTable_{nextMonth.Month}_{nextMonth.Year}") : "\0", });

            rows.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Меню", Constants.GoMenu), });

            markup = new InlineKeyboardMarkup(rows);
            SavedCalendars.Add(currentDate.Month + "_" + currentDate.Year, markup);
            #endregion
        }

        // Send message
        await botClient.SendTextMessageAsync(
            message.Chat.Id,
            "Выберите дату:",
            replyMarkup: markup,
            cancellationToken: cancellationToken);
    }
}
