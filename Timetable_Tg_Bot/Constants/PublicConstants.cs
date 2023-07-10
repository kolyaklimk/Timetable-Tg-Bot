using Telegram.Bot.Types.ReplyMarkups;

namespace TimetableTgBot.Constants;

public class PublicConstants
{
    #region Menu
    public const string ImageMenu = "Изображение";
    public const string SupportMenu = "Поддержка";
    public const string SubscribeMenu = "Подписка";
    public const string GoMenu = "GoMenu";
    #endregion

    #region TimeTable
    public const string ChooseMonthTimeTable = @"^TA_(.{2})_(.{4})$"; // month_year
    public const string ChooseHourTimeTable = @"^TB_(.{2})_(.{2})_(.{4})$"; // day_month_year
    public const string ChooseMinuteTimeTable = @"^TC_(.{2})_(.{2})_(.{2})_(.{4})$"; // hour_day_month_year
    public const string ChooseIsBusyTimeTable = @"^TD_(.{2})_(.{2})_(.{2})_(.{2})_(.{4})$"; // minute_hour_day_month_year
    public const string AddDescriptionTimeTable = @"^TE_(.)_(.{2})_(.{2})_(.{2})_(.{2})_(.{4})$";// true\false_minute_hour_day_month_year
    public const string SaveTimeTable = @"^TF_(.)_(.{2})_(.{2})_(.{2})_(.{2})_(.{4})$";// true\false_minute_hour_day_month_year
    public const string MenuDayTimeTable = @"^TG_(.{2})_(.{2})_(.{4})$"; // day_month_year
    public const string MenuTimeTable = "TH";
    #endregion

    #region Dates
    public const string Month = "Месяц";
    public const string Week = "Неделя";
    #endregion

    #region Other
    public const string Empty = "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";
    public const string dateFormat = "dd/MM/yyyy";
    public const string timeFormat = "HH:mm";

    public static readonly InlineKeyboardButton[] EmptyInlineKeyboardButton = { Empty, "\0" };

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
