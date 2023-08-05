namespace TimetableTgBot.Entities;

public class UserBuffer
{
    public long Id { get; set; } // Id == UserId
    public bool WaitingForText { get; set; } = false;
    public string? Buffer1 { get; set; }
    public int? Buffer2 { get; set; }
    public string? Buffer3 { get; set; }
    public List<DateOnly> ImageDays { get; set; } = new(); // 1 - first day; 2 - last day; Next - deleted days
}
