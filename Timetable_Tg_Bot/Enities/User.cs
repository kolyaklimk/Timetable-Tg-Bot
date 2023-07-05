using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timetable_Tg_Bot.Enities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public ICollection<WorkTime> WorkTimes { get; } = new List<WorkTime>();
    public DateTime Subscription { get; set; }
}
