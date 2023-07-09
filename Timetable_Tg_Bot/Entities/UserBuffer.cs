﻿namespace TimetableTgBot.Entities;

public class UserBuffer
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public string? Buffer1 { get; set; }
    public int? Buffer2 { get; set; }
    public string? Buffer3 { get; set; }
}