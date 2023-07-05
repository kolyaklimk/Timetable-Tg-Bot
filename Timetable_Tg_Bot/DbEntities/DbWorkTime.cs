using TimetableTgBot.Entities;

namespace TimetableTgBot.DbEntities;

public class DbWorkTime : BotDbContext
{
    public void Delete(WorkTime workTime)
    {
        WorkTimes.Remove(workTime);
    }
}