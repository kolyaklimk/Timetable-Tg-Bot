namespace TimetableTgBot.Entities;

public class UserState
{
    public int Id { get; set; }
    public User User { get; set; } = null!;
    public bool WaitingForText { get; set; } = false;
    public string? Buffer1 { get; set; }
    public int? Buffer2 { get; set; }
}
