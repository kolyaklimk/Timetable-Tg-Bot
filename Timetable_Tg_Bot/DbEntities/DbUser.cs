using TimetableTgBot.Entities;

namespace TimetableTgBot.DbEntities;

public class DbUser : BotDbContext
{
    public async Task AddUserAsync(int userId, string name, string password)
    {
        await Users.AddAsync(new User
        {
            Id = userId,
            Name = name,
            Password = password,
            Subscription = DateTime.Now.AddDays(3)
        });
    }

    public async Task AddWorkTimeAsync(User user, DateOnly date, TimeOnly start, TimeOnly end, bool isBusy)
    {
        await WorkTimes.AddAsync(new WorkTime
        {
            User = user,
            UserId = user.Id,
            Date = date,
            Start = start,
            End = end,
            IsBusy = isBusy
        });
    }
}
