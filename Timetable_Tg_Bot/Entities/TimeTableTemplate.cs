namespace TimetableTgBot.Entities;

public class TimeTableTemplate
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public ICollection<WorkTime> Template { get; } = new List<WorkTime>();
}
