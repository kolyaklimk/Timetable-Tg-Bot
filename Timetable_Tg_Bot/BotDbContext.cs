using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using TimetableTgBot.Constants;
using TimetableTgBot.Entities;
using User = TimetableTgBot.Entities.User;
using UserTg = Telegram.Bot.Types.User;

public class BotDbContext: DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<WorkTime> WorkTimes { get; set; }
    public DbSet<UserBuffer> UserBuffers { get; set; }
    public DbSet<TimeTableTemplate> TimeTableTemplates { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(PrivateConstants.DB_TOKEN);
    }

    public async Task<UserBuffer?> GetUserStateAsync(UserTg user)
    {
        return await UserBuffers.FirstOrDefaultAsync(arg => arg.User.Id == user.Id);
    }

    public async Task<User?> GetUserAsync(UserTg user)
    {
        return await Users.FirstOrDefaultAsync(arg => arg.Id == user.Id);
    }

    public async Task<UserBuffer?> GetUserBufferAsync(UserTg user)
    {
        return await UserBuffers.FirstOrDefaultAsync(arg => arg.UserId == user.Id);
    }

    public async Task<TimeTableTemplate?> GetTimeTableTemplateAsync(UserTg user)
    {
        return await TimeTableTemplates.FirstOrDefaultAsync(arg => arg.UserId == user.Id);
    }

    public void UpdateUserStateAsync(UserBuffer userState, bool waitingForText)
    {
        userState.WaitingForText = waitingForText;
    }

    public async Task UpdateUserBuffer_1_2_Async(CallbackQuery callbackQuery)
    {
        var userBuffer = await GetUserBufferAsync(callbackQuery.From);
        userBuffer.Buffer1 = callbackQuery.Data;
        userBuffer.Buffer2 = callbackQuery.Message.MessageId;
    }

    public async Task SetNullUserBuffer3(UserTg user)
    {
        var userBuffer = await GetUserBufferAsync(user);
        userBuffer.Buffer3 = null;
    }

    public async Task RegisterUserAsync(Message message)
    {
        await Users.AddAsync(new User
        {
            Id = message.From.Id,
            FirstName = message.From.FirstName,
            LastName = message.From.LastName,
            UserName = message.From.Username,
            Subscription = message.Date.AddDays(3)
        });

        await UserBuffers.AddAsync(new UserBuffer { UserId = message.From.Id });
    }
}