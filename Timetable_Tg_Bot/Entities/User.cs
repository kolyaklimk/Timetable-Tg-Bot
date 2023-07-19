namespace TimetableTgBot.Entities;

public class User
{
    public long Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }
    public ICollection<WorkTime> WorkTimes { get; } = new List<WorkTime>();
    public ICollection<TimeTableTemplate> TimeTableTemplates { get; } = new List<TimeTableTemplate>();
    public DateTime Subscription { get; set; }
}
