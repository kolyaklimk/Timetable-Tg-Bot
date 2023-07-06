namespace TimetableTgBot.Entities;

public class User
{
    public long Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }
    public ICollection<WorkTime> WorkTimes { get; } = new List<WorkTime>();
    public DateTime Subscription { get; set; }
}
