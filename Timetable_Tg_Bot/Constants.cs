using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
