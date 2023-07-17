using Telegram.Bot.Types.ReplyMarkups;

namespace TimetableTgBot.Constants;

public class PublicConstants
{
    #region Menu
    public const string ImageMenu = "Изображение";
    public const string SupportMenu = "Поддержка";
    public const string SubscribeMenu = "Подписка";
    public const string GoMenu = "MM";
    #endregion

    #region TimeTable
    public const string ChooseMonthTimeTable = @"^TA_(.{2})_(.{4})$"; // month_year
    public const string ChooseHourTimeTable = @"^T(.)_(.{2})_(.{2})_(.{4})$"; // (B_M)_day_month_year
    public const string ChooseMinuteTimeTable = @"^T(.)_(.{2})_(.{2})_(.{2})_(.{4})$"; // (C_N)_hour_day_month_year
    public const string ChooseIsBusyTimeTable = @"^T(.)_(.{2})_(.{2})_(.{2})_(.{2})_(.{4})$"; // (D_O)_minute_hour_day_month_year
    public const string AddDescriptionTimeTable = @"^TE(.)_(.)_(.{2})_(.{2})_(.{2})_(.{2})_(.{4})$";// deleteDescription_true\false_minute_hour_day_month_year
    public const string SaveTimeTable = @"^TF_(.)_(.{2})_(.{2})_(.{2})_(.{2})_(.{4})$";// true\false_minute_hour_day_month_year
    public const string MenuDayTimeTable = @"^TG_(.{2})_(.{2})_(.{4})$"; // day_month_year
    public const string MenuTimeTable = "TH";
    public const string DeleteDayTimeTable = @"^TI_(.{2})_(.{2})_(.{4})$"; // day_month_year
    public const string ChooseTimeTimeTable = @"^TJ_(.{2})_(.{2})_(.{4})$"; // day_month_year
    public const string EditTimeTimeTable = @"^TK(.)_(\w*)$"; // idWirkTime
    public const string MenuTemplateTimeTable = @"^TL_(.{2})_(.{2})_(.{4})$"; // day_month_year
    public const string CreateNewTemplateTimeTable = @"^TP_(.)_(.{2})_(.{2})_(.{2})_(.{2})_(.{4})$"; // true\false_minute_hour_day_month_year
    #endregion

    #region Dates
    public const string Month = "Месяц";
    public const string Week = "Неделя";
    #endregion

    #region Other
    public const string dateFormat = "dd/MM/yyyy";
    public const string timeFormat = "HH:mm";

    public static readonly InlineKeyboardButton[] EmptyInlineKeyboardButton = {
        InlineKeyboardButton.WithCallbackData("…______________________________________________________________________________________", "\0")};

    public static readonly InlineKeyboardMarkup MenuMarkup = new(new[]
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
