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
    public const string ChooseMonthTimeTable = @"^TCMo_(.{1,2})_(.{4})$"; // month_year
    public const string ChooseHourTimeTable = @"^TCH_(.{1,2})_(.{1,2})_(.{4})$"; // day_month_year
    public const string ChooseMinuteTimeTable = @"^TCMi_(.{2})_(.{1,2})_(.{1,2})_(.{4})$"; // hour_day_month_year
    public const string ChooseIsBusyTimeTable = @"^TCIB_(.{2})_(.{2})_(.{1,2})_(.{1,2})_(.{4})$"; // minute_hour_day_month_year
    public const string AddDescriptionTimeTable = @"^TAD_(.)_(.{2})_(.{2})_(.{1,2})_(.{1,2})_(.{4})$";// true\false_minute_hour_day_month_year
    public const string SaveTimeTable = @"^TS_(.)_(.{2})_(.{2})_(.{1,2})_(.{1,2})_(.{4})_(\w*)$";// true\false_minute_hour_day_month_year_description
    public const string MenuDayTimeTable = @"^TMD_(.{1,2})_(.{1,2})_(.{4})$"; // day_month_year
    public const string MenuTimeTable = "TM";
    #endregion

    #region Dates
    public const string Month = "Месяц";
    public const string Week = "Неделя";
    #endregion

    #region Other
    public const string Empty = "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";
    public static readonly InlineKeyboardButton[] EmptyInlineKeyboardButton = { Empty, "\0"};

    public static readonly InlineKeyboardMarkup MenuMarkup = new InlineKeyboardMarkup(new[]
    {
        new InlineKeyboardButton[] {
            ImageMenu,
            InlineKeyboardButton.WithCallbackData("Расписание", MenuTimeTable) },
        new InlineKeyboardButton[] {
            SupportMenu,
            SubscribeMenu },
        EmptyInlineKeyboardButton,
    });

    public static readonly InlineKeyboardMarkup TimeTableMenuMarkup = new InlineKeyboardMarkup(new[]
    {
        new InlineKeyboardButton[] {
            InlineKeyboardButton.WithCallbackData("Просмотр", "\0"),
            InlineKeyboardButton.WithCallbackData("Добавить", $"TCMo_{DateTime.Now.Month}_{DateTime.Now.Year}") },
        EmptyInlineKeyboardButton,
        new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Меню", GoMenu), },
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
