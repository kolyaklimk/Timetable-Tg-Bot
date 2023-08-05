using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using Telegram.Bot.Types;
using TimetableTgBot.Constants;
using TimetableTgBot.Entities;
using User = TimetableTgBot.Entities.User;
using UserTg = Telegram.Bot.Types.User;

public class BotDbContext : DbContext
{
    private EntityEntry<UserBuffer>? EntryBuffer { get; set; }
    private UserBuffer? UpdateUserBuffer { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<WorkTime> WorkTimes { get; set; }
    public DbSet<UserBuffer> UserBuffers { get; set; }
    public DbSet<TimeTableTemplate> TimeTableTemplates { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(PrivateConstants.DB_TOKEN);
    }

    public async Task<bool> UserExistsAsync(UserTg user)
    {
        return await Users.AsNoTracking().AnyAsync(arg => arg.Id == user.Id);
    }

    public async Task<bool> GetUserStateAsync(UserTg user)
    {
        return await UserBuffers
            .AsNoTracking()
            .Where(arg => arg.Id == user.Id)
            .Select(arg => arg.WaitingForText)
            .FirstOrDefaultAsync();
    }

    public async Task<UserBuffer?> GetUserBuffersAsync(UserTg user, Expression<Func<UserBuffer, UserBuffer>> select)
    {
        return await UserBuffers
            .AsNoTracking()
            .Where(arg => arg.Id == user.Id)
            .Select(select)
            .FirstOrDefaultAsync();
    }
    public async Task<List<DateOnly>?> GetUserImageAsync(UserTg user)
    {
        return await UserBuffers
            .AsNoTracking()
            .Where(arg => arg.Id == user.Id)
            .Select(arg => arg.ImageDays)
            .FirstOrDefaultAsync();
    }

    public void UpdateUserStateAsync(UserTg user, bool waitingForText)
    {
        if (EntryBuffer == null)
            SetEntryBuffer(user);
        UpdateUserBuffer.WaitingForText = waitingForText;
        EntryBuffer.Property(x => x.WaitingForText).IsModified = true;
    }

    public void UpdateUserImageAsync(UserTg user, List<DateOnly> dates)
    {
        if (EntryBuffer == null)
            SetEntryBuffer(user);
        UpdateUserBuffer.ImageDays = dates;
        EntryBuffer.Property(x => x.ImageDays).IsModified = true;
    }

    public void UpdateUserBuffer1(UserTg user, string buf)
    {
        if (EntryBuffer == null)
            SetEntryBuffer(user);
        UpdateUserBuffer.Buffer1 = buf;
        EntryBuffer.Property(x => x.Buffer1).IsModified = true;

    }

    public void UpdateUserBuffer2(UserTg user, int buf)
    {
        if (EntryBuffer == null)
            SetEntryBuffer(user);
        UpdateUserBuffer.Buffer2 = buf;
        EntryBuffer.Property(x => x.Buffer2).IsModified = true;
    }

    public void UpdateUserBuffer3(UserTg user, string buf)
    {
        if (EntryBuffer == null)
            SetEntryBuffer(user);
        UpdateUserBuffer.Buffer3 = buf;
        EntryBuffer.Property(x => x.Buffer3).IsModified = true;
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

        await UserBuffers.AddAsync(new UserBuffer { Id = message.From.Id });
    }

    private void SetEntryBuffer(UserTg user)
    {
        UpdateUserBuffer = new UserBuffer { Id = user.Id, };
        UserBuffers.Attach(UpdateUserBuffer);
        EntryBuffer = Entry(UpdateUserBuffer);
    }
}