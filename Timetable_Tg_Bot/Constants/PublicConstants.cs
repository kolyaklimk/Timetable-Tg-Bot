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
    public const string ChooseDay = @"^(.{2})(.{2})(.{4})(\w*)$"; // (TA_IA_IB)_month_year_otherInfo
    #endregion

    #region TimeTable
    public const string AddDescriptionTimeTable = @"^T(.)(.)(.)(.{2})(.{2})(.{2})(.{2})(.{4})(\w*)$";// (C)_deleteDescription_true\false_minute_hour_day_month_year
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
    public const string RangeOfDaysImage = @"^IR(.)(.{0,2})(.{0,2})(.{0,4})(.{0,2})(.{0,2})(.{0,4})$"; // property_day_month_year_day_month_year
    public const string ChooseTemplateImage = "IC";
    public const string EditTemplateImage = @"^IH(.)(.)(.)(.)(.)(.)(.)$"; // theme_background(Y/N)_font_fontColor_color_backroundTheme_position
    public const string CreateImage = @"^I(.)(.)(.)(.)(.)(.)(.)(.)$"; // (L_P)_theme_background(Y/N)_font_fontColor_color_backroundTheme_position

    public const int CountTemplatesImage = 1;
    public const int CountBackgroundImage = 4;
    #endregion

    #region Other
    public const char Okey = '✅';
    public static readonly string[] EditNameSettingsImage = new[] { "Шрифт", "Ц Текст", "Ц Фон1" };
    public static readonly string[] Months = new[] { "", "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
    public static readonly string[] CrossLittleNumbers = new[]
    {
        "₀̶", "₁̶", "₂̶", "₃̶", "₄̶", "₅̶", "₆̶", "₇̶", "₈̶", "₉̶",
        "₁̶₀̶", "₁̶₁̶", "₁̶₂̶", "₁̶₃̶", "₁̶₄̶", "₁̶₅̶", "₁̶₆̶", "₁̶₇̶", "₁̶₈̶", "₁̶₉̶",
        "₂̶₀̶", "₂̶₁̶", "₂̶₂̶", "₂̶₃̶","₂̶₄̶", "₂̶₅̶", "₂̶₆̶", "₂̶₇̶", "₂̶₈̶", "₂̶₉̶",
        "₃̶₀̶", "₃̶₁̶"
    };
    public static readonly string[] BoldNumbers = new[]
    {
        "𝟬", "𝟭", "𝟮", "𝟯", "𝟰", "𝟱", "𝟲", "𝟳", "𝟴", "𝟵",
        "𝟭𝟬", "𝟭𝟭", "𝟭𝟮", "𝟭𝟯", "𝟭𝟰", "𝟭𝟱", "𝟭𝟲", "𝟭𝟳", "𝟭𝟴", "𝟭𝟵",
        "𝟮𝟬", "𝟮𝟭", "𝟮𝟮", "𝟮𝟯", "𝟮𝟰", "𝟮𝟱", "𝟮𝟲", "𝟮𝟳", "𝟮𝟴", "𝟮𝟵",
        "𝟯𝟬", "𝟯𝟭"
    };

    public const string DateFormat = "dd/MM/yyyy";
    public const string TimeFormat = "HH:mm";

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
