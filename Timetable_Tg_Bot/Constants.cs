using Telegram.Bot.Types.ReplyMarkups;

namespace TimetableTgBot;

public class Constants
{
    #region Menu
    public const string ImageMenu = "Изображение";
    public const string SupportMenu = "Поддержка";
    public const string SubscribeMenu = "Подписка";
    public const string GoMenu = "GoMenu";
    #endregion

    #region TimeTable
    public const string ChooseMonthTimeTable = @"^CMoTT_(.{1,2})_(.{4})$"; // month_year
    public const string ChooseHourTimeTable = @"^CHTT_(.{1,2})_(.{1,2})_(.{4})$"; // day_month_year
    public const string ChooseMinuteTimeTable = @"^CMiTT_(.{2})_(.{1,2})_(.{1,2})_(.{4})$"; // hour_day_month_year
    public const string ChooseDurationTimeTable = @"^CDTT_(.{2})_(.{2})_(.{1,2})_(.{1,2})_(.{4})$"; // minute_hour_day_month_year
    #endregion

    #region Dates
    public const string Month = "Месяц";
    public const string Week = "Неделя";
    #endregion

    #region Other
    public static readonly InlineKeyboardMarkup MenuMarkup = new InlineKeyboardMarkup(new[]
    {
        new InlineKeyboardButton[] {
            ImageMenu,
            InlineKeyboardButton.WithCallbackData("Расписание", $"CMoTT_{DateTime.Now.Month}_{DateTime.Now.Year}") },
        new InlineKeyboardButton[] {
            SupportMenu,
            SubscribeMenu }
    });

    public static readonly InlineKeyboardButton[] WeekButtons = new InlineKeyboardButton[] {
        InlineKeyboardButton.WithCallbackData("Пн", "\0"),
        InlineKeyboardButton.WithCallbackData("Вт", "\0"),
        InlineKeyboardButton.WithCallbackData("Ср", "\0"),
        InlineKeyboardButton.WithCallbackData("Чт", "\0"),
        InlineKeyboardButton.WithCallbackData("Пт", "\0"),
        InlineKeyboardButton.WithCallbackData("Сб", "\0"),
        InlineKeyboardButton.WithCallbackData("Вс", "\0") };
    #endregion
}
