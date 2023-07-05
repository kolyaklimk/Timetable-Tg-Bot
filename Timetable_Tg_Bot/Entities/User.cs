namespace TimetableTgBot.Entities;

public class User
{
    public long Id { get; set; }
    public string Name { get; set; }
    public ICollection<WorkTime> WorkTimes { get; } = new List<WorkTime>();
    public DateTime Subscription { get; set; }
}
