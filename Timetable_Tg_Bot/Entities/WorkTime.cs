namespace TimetableTgBot.Entities;

public class WorkTime
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public long? TimeTableTemplateId { get; set; }
    public TimeTableTemplate? TimeTableTemplate { get; set; }
    public TimeOnly Start { get; set; }
    public DateOnly Date { get; set; }
    public bool IsBusy { get; set; }
    public string? Description { get; set; } = "";
}
