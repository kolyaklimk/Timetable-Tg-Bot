using ImageMagick;
using SkiaSharp;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TimetableTgBot.Constants;
using TimetableTgBot.Services;

namespace TimetableTgBot.TgCommands;

public static class ImageCommands
{
    public static async Task ImageMenu(CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        var MenuMarkup = new InlineKeyboardMarkup(new[]
        {
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("Выбрать диапазон", $"IA{callbackQuery.Message.Date.Month:00}{callbackQuery.Message.Date.Year}"),
            },
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("Повторить прошлое изображение", $"\0"),
            },
            PublicConstants.EmptyInlineKeyboardButton,
            new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu)
            },
        });

        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            "Меню image:",
            replyMarkup: MenuMarkup);
    }

    public static async Task RangeOfDaysImage(BotDbContext context, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, PublicConstants.RangeOfDaysImage);
        string property = match.Groups[1].Value;

        var ImageDays = await context.GetUserImageAsync(callbackQuery.From);
        switch (property)
        {
            // Save to Buffer
            case "1":
                var date1 = DateOnly.Parse($"{match.Groups[2].Value}/{match.Groups[3].Value}/{match.Groups[4].Value}");
                var date2 = DateOnly.Parse($"{match.Groups[5].Value}/{match.Groups[6].Value}/{match.Groups[7].Value}");

                if (Math.Abs(date1.DayNumber - date2.DayNumber) > 40)
                {
                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Нельзя выбрать больше 40 дней", true);
                    return;
                }

                ImageDays = new()
                {
                    date1 < date2 ? date1 : date2,
                    date1 > date2 ? date1 : date2
                };
                context.UpdateUserImageAsync(callbackQuery.From, ImageDays);
                break;

            // Add deleted date
            case "2":
                ImageDays.Add(DateOnly.Parse($"{match.Groups[2].Value}/{match.Groups[3].Value}/{match.Groups[4].Value}"));
                context.UpdateUserImageAsync(callbackQuery.From, ImageDays);
                break;

            // Remove deleted date
            case "3":
                var index = ImageDays.FindLastIndex(arg => arg == DateOnly.Parse($"{match.Groups[2].Value}/{match.Groups[3].Value}/{match.Groups[4].Value}"));
                ImageDays.RemoveAt(index);
                context.UpdateUserImageAsync(callbackQuery.From, ImageDays);
                break;
        }
        await context.SaveChangesAsync();

        var rows = new List<InlineKeyboardButton[]> { PublicConstants.WeekButtons };
        var dayCount = ImageDays[1].DayNumber - ImageDays[0].DayNumber;

        HashSet<DateOnly> deletedDays = new();
        for (var i = 2; i < ImageDays.Count; i++)
            deletedDays.Add(ImageDays[i]);

        // Days
        var currentDay = ImageDays[0];
        var currentMonth = currentDay.Month - 1;
        int firstDayOfMonth = 0;
        int rowCount = 0;
        while (currentDay <= ImageDays[1])
        {
            if (currentDay.Month != currentMonth)
            {
                currentMonth = currentDay.Month;
                firstDayOfMonth = ((int)currentDay.DayOfWeek + 6) % 7;
                rowCount = rows.Count + 1;
                rows.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(PublicConstants.Months[currentMonth], "\0") });
            }

            var row = new InlineKeyboardButton[7];
            for (int i = 0; i < 7; i++)
            {
                if (currentDay <= ImageDays[1] && (i >= firstDayOfMonth || rows.Count > rowCount) && currentDay.Month == currentMonth)
                {
                    if (!deletedDays.Contains(currentDay))
                    {
                        row[i] = InlineKeyboardButton.WithCallbackData(currentDay.Day.ToString(), $"IR2{currentDay.Day:00}{currentDay.Month:00}{currentDay.Year}");
                    }
                    else
                    {
                        row[i] = InlineKeyboardButton.WithCallbackData(PublicConstants.CrossLittleNumbers[currentDay.Day], $"IR3{currentDay.Day:00}{currentDay.Month:00}{currentDay.Year}");
                    }
                    currentDay = currentDay.AddDays(1);
                }
                else
                {
                    row[i] = "\0";
                }
            }
            rows.Add(row);
        }

        rows.Add(new[] { InlineKeyboardButton.WithCallbackData("Продолжить", "IC0") });
        rows.Add(PublicConstants.EmptyInlineKeyboardButton);
        rows.Add(new[] {
            InlineKeyboardButton.WithCallbackData("Назад", $"IB{ImageDays[1].Month:00}{ImageDays[1].Year}{ImageDays[0].Day:00}{ImageDays[0].Month:00}{ImageDays[0].Year}"),
            InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu),
        });

        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            "Выбрать откуда начинать:",
            replyMarkup: new InlineKeyboardMarkup(rows));
    }

    public static async Task ChooseTemplateImage(BotDbContext context, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, PublicConstants.ChooseTemplateImage);
        string backround = match.Groups[1].Value;

        var rows = new List<InlineKeyboardButton[]>();

        for (var i = 0; i < PublicConstants.CountTemplatesImage;)
        {
            var row = new InlineKeyboardButton[(PublicConstants.CountTemplatesImage - i) >= 5 ? 5 : (PublicConstants.CountTemplatesImage - i) % 5];
            for (var j = 0; j < row.Length; j++)
            {
                row[j] = InlineKeyboardButton.WithCallbackData(j.ToString(), $"IH{j}{backround}0000a");
                i++;
            }
            rows.Add(row);
        }

        rows.Add(new InlineKeyboardButton[]
        {
            backround=="0"
            ? InlineKeyboardButton.WithCallbackData("Фон - ❌", "IC1")
            : InlineKeyboardButton.WithCallbackData("Фон - ✅", "IC0")
        });
        rows.Add(PublicConstants.EmptyInlineKeyboardButton);
        rows.Add(new[] {
            InlineKeyboardButton.WithCallbackData("Назад", $"IR0"),
            InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu),
        });

        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            "Выберите шаблон картинки\n" +
            $"[Нажми для лучшего качества]({PrivateConstants.TemplateImage})\n",
            replyMarkup: new InlineKeyboardMarkup(rows),
            parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
    }

    public static async Task EditTemplateImage(CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, PublicConstants.EditTemplateImage);
        string backround = match.Groups[2].Value;
        string backroundTheme = match.Groups[6].Value;
        string position = match.Groups[7].Value;

        var rows = new List<InlineKeyboardButton[]>();

        for (int i = 0; i < 3; i++)
        {
            var buttons = new InlineKeyboardButton[4];
            buttons[0] = InlineKeyboardButton.WithCallbackData(PublicConstants.EditNameSettingsImage[i], "\0");
            for (int j = 0; j < 3; j++)
            {
                if (match.Groups[i + 3].Value == j.ToString())
                    buttons[j + 1] = InlineKeyboardButton.WithCallbackData($"✅{j}", "\0");
                else
                    buttons[j + 1] = InlineKeyboardButton.WithCallbackData(j.ToString(), $"{match.Groups[0].Value[..(4 + i)]}{j}{match.Groups[0].Value[(5 + i)..]}");
            }
            rows.Add(buttons);
        }

        if (backround != "0")
        {
            var buttons1 = new InlineKeyboardButton[4];
            buttons1[0] = InlineKeyboardButton.WithCallbackData("Фон", "\0");
            for (int j = 0; j < 3; j++)
            {
                if (backroundTheme == j.ToString())
                    buttons1[j + 1] = InlineKeyboardButton.WithCallbackData($"✅{j}", "\0");
                else
                    buttons1[j + 1] = InlineKeyboardButton.WithCallbackData(j.ToString(), $"{match.Groups[0].Value[..7]}{j}{match.Groups[0].Value[8..]}");
            }
            rows.Add(buttons1);

            char c = 'a';
            for (int i = 0; i < 5; i++)
            {
                var buttons2 = new InlineKeyboardButton[5];
                buttons2[0] = InlineKeyboardButton.WithCallbackData("|", "\0");
                for (int j = 0; j < 3; j++)
                {
                    if (position == c.ToString())
                        buttons2[j + 1] = InlineKeyboardButton.WithCallbackData($"✅", "\0");
                    else
                        buttons2[j + 1] = InlineKeyboardButton.WithCallbackData("-", $"{match.Groups[0].Value[..8]}{c}");
                    c++;
                }
                buttons2[4] = InlineKeyboardButton.WithCallbackData("|", "\0");
                rows.Add(buttons2);
            }
        }
        if (backroundTheme == "2")
        {
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData("Продолжить", $"IL{match.Groups[0].Value[2..]}") });
        }
        else
        {
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData("Продолжить", $"IP{match.Groups[0].Value[2..]}") });
        }

        rows.Add(PublicConstants.EmptyInlineKeyboardButton);
        rows.Add(new[] {
            InlineKeyboardButton.WithCallbackData("Назад", $"IC{backround}"),
            InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu),
        });

        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            "Настройка картинки\n" +
            $"[Нажми для лучшего качества]({PrivateConstants.TemplateImage})\n",
            replyMarkup: new InlineKeyboardMarkup(rows),
            parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
    }

    public static async Task LoadUserImage(string data, ITelegramBotClient botClient, Chat chat, int messageId, string text = null)
    {
        var rows = new List<InlineKeyboardButton[]>
        {
            PublicConstants.EmptyInlineKeyboardButton,
            new[] {
            InlineKeyboardButton.WithCallbackData("Назад", $"IH{data[2..]}"),
            InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu),
            }
        };

        await botClient.EditMessageTextAsync(
            chat.Id,
            messageId,
            $"Отправь изображение *ДОКУМЕНТОМ*\n{text}",
            replyMarkup: new InlineKeyboardMarkup(rows),
            parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
    }

    public static async Task PrintProgress(CallbackQuery callbackQuery, ITelegramBotClient botClient, string text = null)
    {
        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            $"Создание изображения\nВ процессе: {text}",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
    }

    public static async Task CreateImage(string data, BotDbContext context, User user, Chat chat, int messageId, ITelegramBotClient botClient, Update update = null)
    {
        Match match = Regex.Match(data, PublicConstants.CreateImage);
        string theme = match.Groups[2].Value;
        string backround = match.Groups[3].Value;
        string font = match.Groups[4].Value;
        string fontColor = match.Groups[5].Value;
        string сolor = match.Groups[6].Value;
        string backroundTheme = match.Groups[7].Value;
        string position = match.Groups[8].Value;

        SKBitmap bitmapTimeTable = null;

        switch (theme)
        {
            case "0":
                bitmapTimeTable = await ImageGeneration.CreateTimeTableV1(user, context);
                break;

            case "1":
                break;

            case "2":
                break;
        }

        if (backround == "0")
        {
            // Create only timetable
        }
        else
        {
            MagickImage backroundImage = null;
            switch (backroundTheme)
            {
                // Random Image
                case "0":
                    break;

                // Gradient
                case "1":
                    backroundImage = await ImageGeneration.CreateGradient();
                    break;

                // User Image
                case "2":
                    using (var stream = new MemoryStream())
                    {
                        await botClient.GetInfoAndDownloadFileAsync(update.Message.Document.FileId, stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        backroundImage = new MagickImage(stream);
                    }
                    break;
            }
            using (MagickImage timeTableImage = new MagickImage(bitmapTimeTable.Encode(SKEncodedImageFormat.Png, 0).ToArray()))
            {
                ImageGeneration.MergeBackgroundAndTimeTablbe(position, backroundImage, timeTableImage);
            }
            backroundImage.Dispose();
        }
        bitmapTimeTable.Dispose();

        /*await botClient.EditMessageTextAsync(
            chat.Id,
            messageId,
            $"Создание изображения\nВ процессе:",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);*/
    }

}