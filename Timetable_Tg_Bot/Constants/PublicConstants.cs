using Telegram.Bot.Types.ReplyMarkups;

namespace TimetableTgBot.Constants;

public class PublicConstants
{
    #region Menu
    public const string ImageMenu = "IM";
    public const string SupportMenu = "Поддержка";
    public const string SubscribeMenu = "Подписка";
    public const string GoMenu = "MM";
    #endregion

    #region General
    public const string ChooseDay = @"^(.{2})(.{2})(.{4})(\w*)$"; // (TA)_month_year_otherInfo
    #endregion

    #region TimeTable
    public const string ChooseHourTimeTable = @"^T(.)(.{2})(.{2})(.{4})(\w*)$"; // (B_M_U)_day_month_year_otherInfo
    public const string ChooseMinuteTimeTable = @"^T(.)(.{2})(.{2})(.{2})(.{4})(\w*)$"; // (C_N_V)_hour_day_month_year_otherInfo
    public const string ChooseIsBusyTimeTable = @"^T(.)(.{2})(.{2})(.{2})(.{2})(.{4})(\w*)$"; // (D_O_W)_minute_hour_day_month_year_otherInfo
    public const string AddDescriptionTimeTable = @"^TE(.)(.)(.{2})(.{2})(.{2})(.{2})(.{4})$";// deleteDescription_true\false_minute_hour_day_month_year
    public const string SaveTimeTable = @"^TF(.)(.{2})(.{2})(.{2})(.{2})(.{4})$";// true\false_minute_hour_day_month_year
    public const string MenuDayTimeTable = @"^TG(.{2})(.{2})(.{4})$"; // day_month_year
    public const string DeleteDayTimeTable = @"^TI(.{2})(.{2})(.{4})$"; // day_month_year
    public const string ChooseTimeTimeTable = @"^TJ(.{2})(.{2})(.{4})$"; // day_month_year
    public const string EditTimeTimeTable = @"^TK(.)(\w*)$"; // idWirkTime
    public const string CreateNewTemplateTimeTable = @"^TP(.)(.{2})(.{2})(.{2})(.{2})(.{4})$"; // true\false_minute_hour_day_month_year
    public const string ChooseTemplateTimeTable = @"^TQ(.{2})(.{2})(.{4})$"; // day_month_year
    public const string TemplateTimeTable = @"^TR(.)(.{2})(.{2})(.{4})(\w*)$"; // property_day_month_year_IdTemplate
    public const string EditTemplateTimeTable = @"^TS(.{2})(.{2})(.{4})(\w*)$"; // day_month_year_IdTemplate
    public const string EditTimeTemplateTimeTable = @"^TT(.)(.{2})(.{2})(.{4})(\w*)$"; // property_day_month_year_IdWorkTime
    public const string SaveNewTimeTemplateTimeTable = @"^TX(.)(.{2})(.{2})(.{2})(.{2})(.{4})(\w*)$"; // true\false_minute_hour_day_month_year_IdWorkTime
    #endregion

    #region Image
    public const string RangeDaysImage = "IR(.{2})"; // numberOfDays
    public const string ChooseMonthImage = "IC(.{4})"; // year
    #endregion

    #region Other
    public const string dateFormat = "dd/MM/yyyy";
    public const string timeFormat = "HH:mm";

    public static readonly InlineKeyboardButton[] EmptyInlineKeyboardButton = {
        InlineKeyboardButton.WithCallbackData("…______________________________________________________________________________________", "\0")};

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
