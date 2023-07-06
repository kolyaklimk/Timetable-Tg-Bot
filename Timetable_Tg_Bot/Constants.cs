using Telegram.Bot.Types.ReplyMarkups;

namespace TimetableTgBot;

public class Constants
{
    #region Menu
    public const string ChooseMonthTimeTable = @"^ChooseMonthTimeTable_(\d{1,2})_(\d{4})$";
    public const string ImageMenu = "Изображение";
    public const string SupportMenu = "Поддержка";
    public const string SubscribeMenu = "Подписка";
    public const string GoMenu = "GoMenu";
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
            InlineKeyboardButton.WithCallbackData("Расписание", $"ChooseMonthTimeTable_{DateTime.Now.Month}_{DateTime.Now.Year}") },

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
