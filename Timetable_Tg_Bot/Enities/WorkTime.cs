namespace Timetable_Tg_Bot.Enities;

public class WorkTime
{
    public TimeOnly Start { get; set; }
    public TimeOnly End { get; set; }
    public DateOnly Date { get; set; }
    public bool IsBusy { get; set; }
    public string UserId { get; set; }
}
