namespace Timetable_Tg_Bot.Enities;

public class WorkTime
{
    public WorkTime(User user, TimeOnly start, TimeOnly end, DateOnly date, bool isBusy)
    {
        User = user;
        Start = start;
        End = end;
        Date = date;
        IsBusy = isBusy;
    }

    public User User { get; set; }
    public TimeOnly Start { get; set; }
    public TimeOnly End { get; set; }
    public DateOnly Date { get; set; }
    public bool IsBusy { get; set; }
}
