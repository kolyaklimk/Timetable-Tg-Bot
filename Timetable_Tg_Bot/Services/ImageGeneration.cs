using ImageMagick;
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

    private static SKBlendMode GetRandomBlendModeWithoutClear(int rand)
    {
        switch (rand)
        {
            case 0:
                return SKBlendMode.Dst;
            case 1:
                return SKBlendMode.SrcOver;
            case 2:
                return SKBlendMode.SrcIn;
            case 3:
                return SKBlendMode.Plus;
            case 4:
                return SKBlendMode.Screen;
            case 5:
                return SKBlendMode.Lighten;
            case 6:
                return SKBlendMode.ColorDodge;
            case 7:
                return SKBlendMode.Color;
            default:
                return SKBlendMode.Luminosity;
        }
    }

    private static SKColor GetRandomColor(Random random)
    {
        var red = (byte)random.Next(0, 255);
        var green = (byte)random.Next(0, 255);
        var blue = (byte)random.Next(0, 255);

        return new SKColor(red, green, blue);
    }

    private static SKColor[] GetRandomColors(int count, Random random)
    {
        var colors = new SKColor[count];
        for (int i = 0; i < count; i++)
        {
            colors[i] = GetRandomColor(random);
        }

        return colors;
    }

    public static async Task<MagickImage> CreateGradient()
    {
        Random random = new Random();

        using (var paint = new SKPaint())
        {
            using (var bitmap = new SKBitmap(1080, 1920))
            {
                using (var canvas = new SKCanvas(bitmap))
                {
                    canvas.Clear();

                    paint.Shader = SKShader.CreateLinearGradient(
                            new SKPoint(random.Next(1080), random.Next(1920)),
                            new SKPoint(random.Next(1080), random.Next(1920)),
                            GetRandomColors(2, random),
                            SKShaderTileMode.Clamp);

                    canvas.DrawRect(new SKRect(0, 0, 1080, 1920), paint);

                    paint.BlendMode = GetRandomBlendModeWithoutClear(random.Next(8));

                    paint.Shader = SKShader.CreateLinearGradient(
                            new SKPoint(random.Next(1080), random.Next(1920)),
                            new SKPoint(random.Next(1080), random.Next(1920)),
                            GetRandomColors(2, random),
                            SKShaderTileMode.Clamp);

                    canvas.DrawRect(new SKRect(0, 0, 1080, 1920), paint);
                }
                return new MagickImage(bitmap.Encode(SKEncodedImageFormat.Png, 0).ToArray());
            }
        }
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
            }
            return bitmap;
        }
    }

    public static async Task MergeBackgroundAndTimeTablbe(string position, MagickImage backround, MagickImage timeTable)
    {
        // backround +-1080x1920

        if (backround.Height != 1920)
            backround.Resize((int)(1920.0 / backround.Height * backround.Width), 1920);

        double widthTimeTable, heightTimeTable;

        if (backround.Width > 1080)
        {
            widthTimeTable = 648; // 1080 * 0.6
        }
        else
        {
            widthTimeTable = backround.Width * 0.6;
        }
        heightTimeTable = widthTimeTable * 2;


        if (timeTable.Height > heightTimeTable)
        {
            widthTimeTable = heightTimeTable / timeTable.Height * timeTable.Width;
        }
        else
        {
            if (timeTable.Width > widthTimeTable)
            {
                heightTimeTable = widthTimeTable / timeTable.Width * timeTable.Height;
            }
            else
            {
                widthTimeTable = timeTable.Width;
                heightTimeTable = timeTable.Height;
            }
        }

        timeTable.Scale((int)widthTimeTable, (int)heightTimeTable);

        var interval = (backround.Height - backround.Width * 0.2 - timeTable.Height) / 4;
        int x = 0, y = 0;
        switch (position)
        {
            // 1/5
            case "a":
                y = (int)(backround.Width * 0.1);
                x = (int)(backround.Width * 0.1);
                break;
            case "b":
                y = (int)(backround.Width * 0.1);
                x = (int)(backround.Width / 2.0 - timeTable.Width / 2.0);
                break;
            case "c":
                y = (int)(backround.Width * 0.1);
                x = (int)(backround.Width - backround.Width * 0.1) - timeTable.Width;
                break;

            // 2/5
            case "d":
                y = (int)(backround.Width * 0.1 + interval);
                x = (int)(backround.Width * 0.1);
                break;
            case "e":
                y = (int)(backround.Width * 0.1 + interval);
                x = (int)(backround.Width / 2.0 - timeTable.Width / 2.0);
                break;
            case "f":
                y = (int)(backround.Width * 0.1 + interval);
                x = (int)(backround.Width - backround.Width * 0.1) - timeTable.Width;
                break;

            // 3/5
            case "g":
                y = (int)(backround.Width * 0.1 + interval * 2);
                x = (int)(backround.Width * 0.1);
                break;
            case "h":
                y = (int)(backround.Width * 0.1 + interval * 2);
                x = (int)(backround.Width / 2.0 - timeTable.Width / 2.0);
                break;
            case "i":
                y = (int)(backround.Width * 0.1 + interval * 2);
                x = (int)(backround.Width - backround.Width * 0.1) - timeTable.Width;
                break;

            // 4/5
            case "j":
                y = (int)(backround.Width * 0.1 + interval * 3);
                x = (int)(backround.Width * 0.1);
                break;
            case "k":
                y = (int)(backround.Width * 0.1 + interval * 3);
                x = (int)(backround.Width / 2.0 - timeTable.Width / 2.0);
                break;
            case "l":
                y = (int)(backround.Width * 0.1 + interval * 3);
                x = (int)(backround.Width - backround.Width * 0.1) - timeTable.Width;
                break;

            // 5/5
            case "m":
                y = (int)(backround.Height - backround.Width * 0.1) - timeTable.Height;
                x = (int)(backround.Width * 0.1);
                break;
            case "n":
                y = (int)(backround.Height - backround.Width * 0.1) - timeTable.Height;
                x = (int)(backround.Width / 2.0 - timeTable.Width / 2.0);
                break;
            case "o":
                y = (int)(backround.Height - backround.Width * 0.1) - timeTable.Height;
                x = (int)(backround.Width - backround.Width * 0.1) - timeTable.Width;
                break;
        }

        backround.Composite(timeTable, x, y, CompositeOperator.SrcOver);
        backround.Write(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"{position}ski.Jpeg"));

        Console.WriteLine("Save");
    }
}
