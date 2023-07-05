namespace Timetable_Tg_Bot.Enities;

public class User
{
    public User(string userId, string name, string password)
    {
        UserId = userId;
        Name = name;
        Password = password;
    }
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public List<WorkTime> WorkTime { get; set; } = new();
    public DateTime Subscription { get; set; } = DateTime.Now;
}
