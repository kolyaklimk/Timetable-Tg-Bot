namespace Timetable_Tg_Bot.Enities;

public class User
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public List<WorkTime> WorkTimes { get; set; } = new();
    public DateTime Subscription { get; set; }
}
