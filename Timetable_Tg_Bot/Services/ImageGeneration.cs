using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System.Text;
using Telegram.Bot.Types;
using TimetableTgBot.Constants;

namespace TimetableTgBot.Services;

public class ImageGeneration
{
    private static async Task<List<DateOnly>> GetDaysForTimeTable(User user, BotDbContext context)
    {
        var days = await context.GetUserBuffersAsync(user, arg => new Entities.UserBuffer { ImageDays = arg.ImageDays });
        var listDays = new List<DateOnly>() { days.ImageDays[0] };
        var buffDay = days.ImageDays[0];
        while (buffDay != days.ImageDays[1])
        {
            buffDay = buffDay.AddDays(1);
            listDays.Add(buffDay);
        }
        for (int i = 2; i < days.ImageDays.Count; i++)
        {
            listDays.Remove(days.ImageDays[i]);
        }
        return listDays;
    }

    public static async Task<SKBitmap> CreateTimeTableV1(User user, BotDbContext context)
    {
        var listDays = await GetDaysForTimeTable(user, context);

        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            List<string> lines = new();
            var dayTextSize = 50;
            var monthTextSize = 70;
            string familyName = "Roboto";
            float buffWidth = 0;
            float width = 0;
            float height = 0;
            int currectMonth = 13;

            foreach (var day in listDays)
            {
                var dayTimetable = await context.WorkTimes
                    .AsNoTracking()
                    .Where(arg => arg.UserId == user.Id && arg.Date == day && !arg.IsBusy)
                    .OrderBy(arg => arg.Start)
                    .ToListAsync();

                if (dayTimetable.Count > 0)
                {
                    if (day.Month != currectMonth)
                    {
                        currectMonth = day.Month;
                        lines.Add($"-{PublicConstants.Months[currectMonth]}");
                        paint.Typeface = SKTypeface.FromFamilyName(familyName, SKFontStyle.Bold);
                        paint.TextSize = monthTextSize;
                        buffWidth = paint.MeasureText(PublicConstants.Months[currectMonth]);
                        paint.TextSize = dayTextSize;
                        paint.Typeface = SKTypeface.FromFamilyName(familyName, SKFontStyle.Normal);

                        if (buffWidth > width)
                            width = buffWidth;
                        height += monthTextSize * 2;
                    }
                    else
                    {
                        height += dayTextSize;
                    }

                    var str = new StringBuilder($"{day:dd.MM} - ");
                    foreach (var time in dayTimetable)
                    {
                        str.Append($"{time.Start:HH:mm}, ");
                    }
                    str.Remove(str.Length - 2, 2);
                    lines.Add(str.ToString());
                    buffWidth = paint.MeasureText(str.ToString());

                    if (buffWidth > width)
                        width = buffWidth;
                }
            }

            width += dayTextSize << 1;
            height += (dayTextSize << 1) - (float)(monthTextSize - monthTextSize / 1.4);
            bool isMonth = false;

            var bitmap = new SKBitmap((int)width, (int)height);

            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear();
                paint.Color = SKColors.White;
                canvas.DrawRoundRect(new SKRoundRect(new SKRect(0, 0, (int)width, (int)height), 50), paint);

                float y = dayTextSize - (float)(monthTextSize - monthTextSize / 1.4);
                foreach (var line in lines)
                {
                    if (line[0] == '-')
                    {
                        y += monthTextSize;
                        paint.Color = SKColors.Black;
                        paint.Typeface = SKTypeface.FromFamilyName(familyName, SKFontStyle.Bold);
                        paint.TextSize = monthTextSize;
                        canvas.DrawText(line[1..], dayTextSize, y, paint);
                        paint.Color = SKColors.Gray;
                        paint.TextSize = dayTextSize;
                        paint.Typeface = SKTypeface.FromFamilyName(familyName, SKFontStyle.Normal);
                        isMonth = true;
                        continue;
                    }
                    if (isMonth)
                    {
                        y += monthTextSize;
                        isMonth = false;
                    }
                    else
                    {
                        y += dayTextSize;
                    }
                    canvas.DrawText(line, dayTextSize, y, paint);
                }

                return bitmap;

                /*using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 0))
                using (var stream = System.IO.File.OpenWrite(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"ski.png")))
                {
                    data.SaveTo(stream);
                }*/
            }
        }
    }

    public static async Task MergeBackgroundAndTimeTablbe(SKBitmap bitmapBackround, SKBitmap bitmapTimeTable)
    {
        // backround size = +-1080 x 1920

        var WidthTimeTable = bitmapBackround.Width * 0.6;
        var HeightTimeTable = WidthTimeTable * 2;

        if (bitmapTimeTable.Height > HeightTimeTable)
        {
            WidthTimeTable = HeightTimeTable / bitmapTimeTable.Height * bitmapTimeTable.Width;
        }
        else
        {
            if (bitmapTimeTable.Width > WidthTimeTable)
            {
                HeightTimeTable = WidthTimeTable / bitmapTimeTable.Width * bitmapTimeTable.Height;
            }
            else
            {
                WidthTimeTable = bitmapTimeTable.Width;
                HeightTimeTable = bitmapTimeTable.Height;
            }
        }

        Console.WriteLine("WidthTimeTable" + WidthTimeTable);
        Console.WriteLine("HeightTimeTable" + HeightTimeTable);
        using (var canvas = new SKCanvas(bitmapBackround))
        {
            canvas.DrawBitmap(bitmapTimeTable, 500, 500);

        }

        using (var image = SKImage.FromBitmap(bitmapBackround))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 0))
        using (var stream = System.IO.File.OpenWrite(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"ski.png")))
        {
            data.SaveTo(stream);
        }

        Console.WriteLine("Save");
    }
}
