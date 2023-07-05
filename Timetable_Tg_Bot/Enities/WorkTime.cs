using Microsoft.EntityFrameworkCore;

namespace Timetable_Tg_Bot.Enities;

public class WorkTime
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public TimeOnly Start { get; set; }
    public TimeOnly End { get; set; }
    public DateOnly Date { get; set; }
    public bool IsBusy { get; set; }
}
