using Microsoft.EntityFrameworkCore;
using Npgsql.Internal.TypeHandlers.GeometricHandlers;
using SkiaSharp;
using System.Runtime.CompilerServices;
using System.Text;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;

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
        for(int i=2;i<days.ImageDays.Count;i++)
        {
            listDays.Remove(days.ImageDays[i]);
        }
        return listDays;
    }

    public static async Task CreateTimeTableV1(User user, BotDbContext context)
    {
        var dayTextSize = 30;
        var monthTextSize = 50;
        var listDays = await GetDaysForTimeTable(user, context);
        SKTypeface font = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal);

        using (var paint = new SKPaint())
        {
            paint.Typeface = font;
            paint.IsAntialias = true;

            List<string> lines = new();
            
            int width = 0;
            int height = 0;

            foreach (var day in listDays)
            {
                var dayTimetable = await context.WorkTimes
                    .AsNoTracking()
                    .Where(arg => arg.UserId == user.Id && arg.Date == day && !arg.IsBusy)
                    .OrderBy(arg => arg.Start)
                    .ToListAsync();

                if(dayTimetable.Count>0)
                {
                    var str = new StringBuilder();

                    str.Append($"{day.Day:dd.MM} - ");

                    foreach(var time in dayTimetable)
                    {
                        str.Append($"{time.Start:HH:mm}, ");
                    }

                    str.Remove(str.Length - 2, 2);
                    
                    var bounds = new SKRect();
                    paint.MeasureText(str.ToString(), ref bounds);

                    if ((int)bounds.Width > width)
                        width = (int)bounds.Width;
                    height++;
                }
            }

            width += textSize * 2;
            height *= textSize;

            using (var bitmap = new SKBitmap(width, height))
            {
                using (var canvas = new SKCanvas(bitmap))
                {
                    canvas.Clear(SKColors.White); // Заливка фона белым цветом

                    paint.Color = SKColors.Black;

                    float y = textSize * 2;

                    foreach (var line in lines)
                    {
                        canvas.DrawText(line, textSize, y, paint);
                        y += textSize; // Увеличиваем y для следующей строки
                    }
                }

                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 0))
                using (var stream = System.IO.File.OpenWrite(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"ski.png")))
                {
                    // save the data to a stream
                    data.SaveTo(stream);
                }
            }
        }

    }
}
