namespace TimetableTgBot.Entities;

public class TimeTableTemplate
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public List<List<WorkTime>> Template { get; set; } = new();
}
