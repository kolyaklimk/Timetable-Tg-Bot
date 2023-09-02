using ImageMagick;
using SkiaSharp;
using System.Text.Json;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TimetableTgBot.Constants;
using TimetableTgBot.Services;

namespace TimetableTgBot.TgCommands;

public static class ImageCommands
{
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

    public static async Task ChooseTemplateImage(CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        var rows = new List<InlineKeyboardButton[]>();

        for (var i = 0; i < PublicConstants.CountTemplatesImage;)
        {
            var row = new InlineKeyboardButton[(PublicConstants.CountTemplatesImage - i) >= 5 ? 5 : (PublicConstants.CountTemplatesImage - i) % 5];
            for (var j = 0; j < row.Length; j++)
            {
                row[j] = InlineKeyboardButton.WithCallbackData(j.ToString(), $"IH{j}10000a");
                i++;
            }
            rows.Add(row);
        }

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
        string theme = match.Groups[1].Value;
        string backround = match.Groups[2].Value;
        string font = match.Groups[3].Value;
        string fontColor = match.Groups[4].Value;
        string color = match.Groups[5].Value;
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
            rows.Add(new InlineKeyboardButton[]
            {
            InlineKeyboardButton.WithCallbackData("Без фона", $"IH{theme}0{font}{fontColor}{color}{backroundTheme}{position}"),
            InlineKeyboardButton.WithCallbackData("✅Фон", "\0")
            });

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
        else
        {
            rows.Add(new InlineKeyboardButton[]
            {
            InlineKeyboardButton.WithCallbackData("✅Без фона", "\0"),
            InlineKeyboardButton.WithCallbackData("Фон", $"IH{theme}1{font}{fontColor}{color}{backroundTheme}{position}")
            });
        }
        if (backroundTheme == "2")
        {
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData("Продолжить", $"IL{match.Groups[0].Value[2..]}") });
        }
        else
        {
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData("Продолжить", $"IP{match.Groups[0].Value[2..]}") });
        }

        rows.Add(new[] {
            InlineKeyboardButton.WithCallbackData("Назад", $"IC{backround}"),
            PublicConstants.EmptyInlineKeyboardButton[0],
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

    public static async Task PrintProgress(Chat chat, int messageId, ITelegramBotClient botClient, string text = null)
    {
        await botClient.EditMessageTextAsync(
            chat.Id,
            messageId,
            $"Создание изображения\nВ процессе: {text}",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
    }

    public static async Task CreateImage(string data, BotDbContext context, User user, Chat chat, int messageId, ITelegramBotClient botClient, Update update = null)
    {
        await PrintProgress(chat, messageId, botClient, "Создаю картинку расписания");
        Match match = Regex.Match(data, PublicConstants.CreateImage);
        string theme = match.Groups[2].Value;
        string backround = match.Groups[3].Value;
        string font = match.Groups[4].Value;
        string fontColor = match.Groups[5].Value;
        string сolor = match.Groups[6].Value;
        string backroundTheme = match.Groups[7].Value;
        string position = match.Groups[8].Value;

        SKBitmap bitmapTimeTable = null;
        MagickImage backroundImage = null;
        var rows = new List<InlineKeyboardButton[]>();
        string link = "";
        var content = new MultipartFormDataContent
        {
            { new StringContent(PrivateConstants.ImgbbKey), "key" },
            { new StringContent("3600"), "expiration" },
        };

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

        using (MagickImage timeTableImage = new MagickImage(bitmapTimeTable.Encode(SKEncodedImageFormat.Png, 0).ToArray()))
        {
            bitmapTimeTable.Dispose();
            if (backround != "0")
            {
                switch (backroundTheme)
                {
                    // Random Image
                    case "0":
                        await PrintProgress(chat, messageId, botClient, "Беру рандомное фото");
                        var random = new Random();
                        string resourceName = $"Resources\\BackgroudImages\\{random.Next(1, PublicConstants.CountBackgroundImage)}.jpg";
                        backroundImage = new MagickImage(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, resourceName));
                        rows.Add(new[] {
                            InlineKeyboardButton.WithCallbackData("Заново", data),
                        });
                        break;

                    // Gradient
                    case "1":
                        await PrintProgress(chat, messageId, botClient, "Создаю градиент");
                        backroundImage = await ImageGeneration.CreateGradient();
                        rows.Add(new[] {
                            InlineKeyboardButton.WithCallbackData("Заново", data),
                        });
                        break;

                    // User Image
                    case "2":
                        await PrintProgress(chat, messageId, botClient, "Скачиваю фон");
                        using (var stream = new MemoryStream())
                        {
                            await botClient.GetInfoAndDownloadFileAsync(update.Message.Document.FileId, stream);
                            stream.Seek(0, SeekOrigin.Begin);
                            backroundImage = new MagickImage(stream);
                        }
                        break;
                }

                await PrintProgress(chat, messageId, botClient, "Объединяю фон с расписанием");
                ImageGeneration.MergeBackgroundAndTimeTablbe(position, backroundImage, timeTableImage);
                content.Add(new StringContent(backroundImage.ToBase64(MagickFormat.Jpeg)), "image");
                backroundImage.Dispose();
            }
            else
            {
                content.Add(new StringContent(timeTableImage.ToBase64(MagickFormat.Png)), "image");
            }
        }

        await PrintProgress(chat, messageId, botClient, "Загружаю картинку");
        using (HttpClient httpClient = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await httpClient.PostAsync("https://api.imgbb.com/1/upload", content);

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var jsonDoc = await JsonSerializer.DeserializeAsync<JsonDocument>(await response.Content.ReadAsStreamAsync(), options);
                    link = jsonDoc.RootElement.GetProperty("data").GetProperty("url").GetString();
                }
                else
                {
                    Console.WriteLine($"Ошибка: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        rows.Add(new[] {
            InlineKeyboardButton.WithCallbackData("Назад", $"IH{data[2..]}"),
            InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu),
            });

        await botClient.EditMessageTextAsync(
            chat.Id,
            messageId,
            $"Скачать в хорошем качестве можно по [ССЫЛКЕ]({link})\\.\n Ссылка действует 1 час\\.",
            replyMarkup: new InlineKeyboardMarkup(rows),
            parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
    }

}