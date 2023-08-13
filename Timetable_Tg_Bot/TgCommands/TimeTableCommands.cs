using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TimetableTgBot.Constants;
using TimetableTgBot.Entities;

namespace TimetableTgBot.TgCommands;

public static class TimeTableCommands
{
    public static async Task MenuDayTimeTable(BotDbContext context, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery.Data, PublicConstants.MenuDayTimeTable);

        string day = match.Groups[1].Value;
        string month = match.Groups[2].Value;
        string year = match.Groups[3].Value;

        DateOnly currentDate = DateOnly.ParseExact($"{day}/{month}/{year}", PublicConstants.DateFormat, null);

        var dayTimetable = await context.WorkTimes
            .AsNoTracking()
            .Where(arg => arg.UserId == callbackQuery.From.Id && arg.Date == currentDate)
            .OrderBy(arg => arg.Start)
            .ToListAsync();

        var stringBuilder = new StringBuilder();
        foreach (var item in dayTimetable)
        {
            stringBuilder.AppendLine($"{item.Start.Hour:00}:{item.Start.Minute:00} \\- {item.IsBusy} \\- {item.Description}\n");
        }

        // previous and next buttons
        var previousMonth = currentDate.AddDays(-1);
        var nextMonth = currentDate.AddDays(1);

        var rows = new InlineKeyboardButton[][] {
            new[]{
                InlineKeyboardButton.WithCallbackData("Изменить время",$"TJ{day}{month}{year}"),
                InlineKeyboardButton.WithCallbackData("Выбрать время", $"TB{day}{month}{year}"),
            },
            new[]{
                InlineKeyboardButton.WithCallbackData("Удалить всё", $"TI{day}{month}{year}"),
                InlineKeyboardButton.WithCallbackData("Выбрать шаблон", $"TQ{day}{month}{year}"),
            },
            new[]{
                currentDate.Year >=  callbackQuery.Message.Date.AddYears(-1).Year ? InlineKeyboardButton.WithCallbackData("<<",$"TG{previousMonth.Day :00}{previousMonth.Month:00}{previousMonth.Year}") : "\0",
                PublicConstants.EmptyInlineKeyboardButton[0],
                currentDate.Year <=  callbackQuery.Message.Date.AddYears(1).Year ? InlineKeyboardButton.WithCallbackData(">>",$"TG{nextMonth.Day :00}{nextMonth.Month :00}{nextMonth.Year}") : "\0",
            },
            new[]{
                InlineKeyboardButton.WithCallbackData("Назад", $"TA{month}{year}"),
                InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu),
            }
        };

        // Send message
        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            $"Вы выбрали: {day}/{month}/{year}\n{stringBuilder}\nВыберите час:",
            replyMarkup: new InlineKeyboardMarkup(rows));
    }

    public static async Task ChooseHourTimeTable(string next, string previous, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, PublicConstants.ChooseHourTimeTable);

        string day = match.Groups[2].Value;
        string month = match.Groups[3].Value;
        string year = match.Groups[4].Value;
        string otherInfo = match.Groups[5].Value;

        // Hours
        var rows = new List<InlineKeyboardButton[]>();

        for (var i = 0; i < 24;)
        {
            var row = new InlineKeyboardButton[6];
            for (var j = 0; j < 6; j++)
            {
                row[j] = InlineKeyboardButton.WithCallbackData($"{i:00}", $"T{next}{i:00}{day}{month}{year}{otherInfo}");
                i++;
            }
            rows.Add(row);
        }

        rows.Add(PublicConstants.EmptyInlineKeyboardButton);
        rows.Add(new[] {
            InlineKeyboardButton.WithCallbackData("Назад", $"T{previous}{day}{month}{year}{otherInfo}"),

            InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu)
        });

        // Send message
        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            $"Вы выбрали: __*{day}/{month}/{year}*__\nВыберите час:",
            replyMarkup: new InlineKeyboardMarkup(rows),
            parseMode: ParseMode.MarkdownV2);
    }

    public static async Task ChooseMinuteTimeTable(string next, string previous, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, PublicConstants.ChooseMinuteTimeTable);

        string hour = match.Groups[2].Value;
        string day = match.Groups[3].Value;
        string month = match.Groups[4].Value;
        string year = match.Groups[5].Value;
        string otherInfo = match.Groups[6].Value;

        // Minute
        var rows = new List<InlineKeyboardButton[]>();

        for (var i = 0; i < 60;)
        {
            var row = new InlineKeyboardButton[6];
            for (var j = 0; j < 6; j++)
            {
                row[j] = InlineKeyboardButton.WithCallbackData($"{i:00}", $"T{next}{i:00}{hour}{day}{month}{year}{otherInfo}");
                i += 5;
            }
            rows.Add(row);
        }
        rows.Add(PublicConstants.EmptyInlineKeyboardButton);
        rows.Add(new[] {
            InlineKeyboardButton.WithCallbackData("Назад", $"T{previous}{day}{month}{year}{otherInfo}"),
            InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu)
        });

        // Send message
        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            "Вы выбрали:\n" +
            $"Дата: __*{day}/{month}/{year}*__\n" +
            $"Час: __*{hour}*__",
            replyMarkup: new InlineKeyboardMarkup(rows),
            parseMode: ParseMode.MarkdownV2);
    }

    public static async Task ChooseIsBusyTimeTable(string next, string previous, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, PublicConstants.ChooseIsBusyTimeTable);

        string minute = match.Groups[2].Value;
        string hour = match.Groups[3].Value;
        string day = match.Groups[4].Value;
        string month = match.Groups[5].Value;
        string year = match.Groups[6].Value;
        string otherInfo = match.Groups[7].Value;

        var rows = new InlineKeyboardButton[][]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Свободно",$"T{next}0{minute}{hour}{day}{month}{year}{otherInfo}"),
                InlineKeyboardButton.WithCallbackData("Запись", $"T{next}1{minute}{hour}{day}{month}{year}{otherInfo}"),
            },
            PublicConstants.EmptyInlineKeyboardButton,
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", $"T{previous}{hour}{day}{month}{year}{otherInfo}"),
                InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu),
            }
        };

        // Send message
        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            $"Вы свободны?\n" +
            $"Дата: {day}/{month}/{year}\n" +
            $"Время: {hour}:{minute}",
            replyMarkup: new InlineKeyboardMarkup(rows),
            parseMode: ParseMode.MarkdownV2);
    }

    public static async Task AddDescriptionTimeTable(string? description, string data, ITelegramBotClient botClient, Chat chat, int messageId)
    {
        Match match = Regex.Match(data, PublicConstants.AddDescriptionTimeTable);

        string isBusy = match.Groups[2].Value;
        string minute = match.Groups[3].Value;
        string hour = match.Groups[4].Value;
        string day = match.Groups[5].Value;
        string month = match.Groups[6].Value;
        string year = match.Groups[7].Value;

        var rows = new InlineKeyboardButton[][]
        {
            string.IsNullOrEmpty(description)
            ? new[]
            {
                InlineKeyboardButton.WithCallbackData("Сохранить описания", $"TF{isBusy}{minute}{hour}{day}{month}{year}"),
            }
            : new[]
            {
                InlineKeyboardButton.WithCallbackData("Удалить описание", $"TEY{isBusy}{minute}{hour}{day}{month}{year}"),
                InlineKeyboardButton.WithCallbackData("Сохранить", $"TF{isBusy}{minute}{hour}{day}{month}{year}"),
            },
            PublicConstants.EmptyInlineKeyboardButton,
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", $"TD{minute}{hour}{day}{month}{year}"),
                InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu),
            }
        };

        // Send message
        await botClient.EditMessageTextAsync(
            chat.Id,
            messageId,
            $"Напишите, если хотите изменить описание к этой записи :\\)\n" +
            $"Дата: {day}/{month}/{year}\n" +
            $"Время: {hour}:{minute}\n" +
            $"Запись: {"1" == isBusy}" + (description != null ? $"\nОписание: {description}" : ""),
            replyMarkup: new InlineKeyboardMarkup(rows),
            parseMode: ParseMode.MarkdownV2);
    }

    public static async Task SaveTimeTable(BotDbContext context, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, PublicConstants.SaveTimeTable);

        string isBusy = match.Groups[1].Value;
        string minute = match.Groups[2].Value;
        string hour = match.Groups[3].Value;
        string day = match.Groups[4].Value;
        string month = match.Groups[5].Value;
        string year = match.Groups[6].Value;

        var userBuffer = await context.GetUserBuffersAsync(callbackQuery.From, arg => new UserBuffer { Buffer3 = arg.Buffer3 });

        await context.WorkTimes.AddAsync(new WorkTime
        {
            Date = DateOnly.ParseExact($"{day}/{month}/{year}", PublicConstants.DateFormat, null),
            Start = TimeOnly.ParseExact($"{hour}:{minute}", PublicConstants.TimeFormat, null),
            IsBusy = "1" == isBusy,
            UserId = callbackQuery.From.Id,
            Description = userBuffer.Buffer3
        });

        context.UpdateUserBuffer3(callbackQuery.From, string.Empty);
        await context.SaveChangesAsync();

        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"Запись на {day}/{month}/{year} в {hour}:{minute} сохранена!", false);

        callbackQuery.Data = $"TG{day}{month}{year}";
        await MenuDayTimeTable(context, callbackQuery, botClient);
    }

    public static async Task DeleteDayTimeTable(BotDbContext context, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, PublicConstants.DeleteDayTimeTable);

        string day = match.Groups[1].Value;
        string month = match.Groups[2].Value;
        string year = match.Groups[3].Value;

        DateOnly currentDate = DateOnly.ParseExact($"{day}/{month}/{year}", PublicConstants.DateFormat, null);

        var dayTimetable = context.WorkTimes.Where(arg => arg.UserId == callbackQuery.From.Id && arg.Date == currentDate);
        context.WorkTimes.RemoveRange(dayTimetable);
        await context.SaveChangesAsync();

        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"Записи на {day}/{month}/{year} удалены!", false);

        callbackQuery.Data = $"TG{day}{month}{year}";
        await MenuDayTimeTable(context, callbackQuery, botClient);
    }

    public static async Task ChooseTimeTimeTable(BotDbContext context, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, PublicConstants.ChooseTimeTimeTable);

        string day = match.Groups[1].Value;
        string month = match.Groups[2].Value;
        string year = match.Groups[3].Value;

        DateOnly currentDate = DateOnly.ParseExact($"{day}/{month}/{year}", PublicConstants.DateFormat, null);

        var times = await context.WorkTimes
            .AsNoTracking()
            .Where(arg => arg.UserId == callbackQuery.From.Id && arg.Date == currentDate)
            .OrderBy(arg => arg.Start)
            .ToListAsync();

        // Times
        var rows = new List<InlineKeyboardButton[]>();

        for (var i = 0; i < times.Count;)
        {
            var row = new InlineKeyboardButton[(times.Count - i) >= 4 ? 4 : (times.Count - i) % 4];
            for (var j = 0; j < row.Length; j++)
            {
                row[j] = InlineKeyboardButton.WithCallbackData(
                    $"{times[i].Start.Hour}:{times[i].Start.Minute:00}",
                    $"TK0{times[i].Id}");
                i++;
            }
            rows.Add(row);
        }

        rows.Add(PublicConstants.EmptyInlineKeyboardButton);
        rows.Add(new[] {
                InlineKeyboardButton.WithCallbackData("Назад", $"TG{day}{month}{year}"),
                InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu),
        });

        // Send message
        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            "Выберите время:",
            replyMarkup: new InlineKeyboardMarkup(rows));
    }

    public static async Task EditTimeTimeTable(string? newDescription, string data, BotDbContext context, ITelegramBotClient botClient, Chat chat, int messageId, CallbackQuery callbackQuery = null)
    {
        Match match = Regex.Match(data, PublicConstants.EditTimeTimeTable);

        string index = match.Groups[1].Value;
        string idWorkTime = match.Groups[2].Value;
        var time = await context.WorkTimes.FirstOrDefaultAsync(arg => arg.Id == long.Parse(idWorkTime));

        switch (index)
        {
            case "D":
                context.WorkTimes.Remove(time);
                await context.SaveChangesAsync();
                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"Запись удалена!", false);

                callbackQuery.Data = $"TG{time.Date.Day:00}{time.Date.Month:00}{time.Date.Year}";
                await MenuDayTimeTable(context, callbackQuery, botClient);
                return;

            case "R":
                if (time.Description == null)
                {
                    botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                    break;
                }
                time.Description = null;
                goto default;

            case "1":
                time.IsBusy = !time.IsBusy;
                goto default;

            default:
                if (newDescription != null)
                    time.Description = newDescription;

                var rows = new InlineKeyboardButton[][]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Удалить",$"TKD{idWorkTime}"),
                        InlineKeyboardButton.WithCallbackData(time.IsBusy ?"Свободно" : "Занято", $"TK1{idWorkTime}"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Удалить описание", $"TKR{idWorkTime}"),
                    },
                    PublicConstants.EmptyInlineKeyboardButton,
                    new[] {
                        InlineKeyboardButton.WithCallbackData("Назад", $"TJ{time.Date.Day :00}{time.Date.Month :00}{time.Date.Year}"),
                        InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu),
                    }
                };

                // Send message
                await botClient.EditMessageTextAsync(
                    chat.Id,
                    messageId,
                    $"Дата: {time.Date.Day}/{time.Date.Month}/{time.Date.Year}\n" +
                    $"Время: {time.Start.Hour}:{time.Start.Minute}\n" +
                    $"Занятость: {time.IsBusy}\n" +
                    $"Описание: {time.Description}\n\n" +
                    "Выбери чёнить.\nНапиши что-нибудь, чтобы изменить описание (не работает):",
                    disableWebPagePreview: true,
                    replyMarkup: new InlineKeyboardMarkup(rows));
                break;
        }
        await context.SaveChangesAsync();
    }

    public static async Task CreateNewTemplateTimeTable(BotDbContext context, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, PublicConstants.CreateNewTemplateTimeTable);

        string isBusy = match.Groups[1].Value;
        string minute = match.Groups[2].Value;
        string hour = match.Groups[3].Value;
        string day = match.Groups[4].Value;
        string month = match.Groups[5].Value;
        string year = match.Groups[6].Value;

        var template = await context.TimeTableTemplates.AddAsync(new TimeTableTemplate
        {
            UserId = callbackQuery.From.Id,
        });
        template.Entity.Template.Add(new WorkTime
        {
            UserId = callbackQuery.From.Id,
            IsBusy = isBusy == "1",
            Start = TimeOnly.ParseExact($"{hour}:{minute}", PublicConstants.TimeFormat, null),
        });
        await context.SaveChangesAsync();

        callbackQuery.Data = $"TR0{day}{month}{year}{template.Entity.Id}";
        await TemplateTimeTable(context, callbackQuery, botClient);
    }

    public static async Task ChooseTemplateTimeTable(BotDbContext context, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, PublicConstants.ChooseTemplateTimeTable);

        string day = match.Groups[1].Value;
        string month = match.Groups[2].Value;
        string year = match.Groups[3].Value;

        var templates = await context.Users
            .AsNoTracking()
            .Where(arg => arg.Id == callbackQuery.From.Id)
            .Select(arg => arg.TimeTableTemplates)
            .FirstOrDefaultAsync();

        // Templates
        var rows = new List<InlineKeyboardButton[]>();

        for (var i = 0; i < templates.Count;)
        {
            var row = new InlineKeyboardButton[(templates.Count - i) >= 6 ? 6 : (templates.Count - i) % 6];
            for (var j = 0; j < row.Length; j++)
            {
                row[j] = InlineKeyboardButton.WithCallbackData(
                    $"{i + 1}",
                    $"TR0{day}{month}{year}{templates.ElementAt(i).Id}");
                i++;
            }
            rows.Add(row);
        }

        rows.Add(new[] { InlineKeyboardButton.WithCallbackData("Создать", $"TM{day}{month}{year}") });
        rows.Add(PublicConstants.EmptyInlineKeyboardButton);
        rows.Add(new[] {
                InlineKeyboardButton.WithCallbackData("Назад", $"TG{day}{month}{year}"),
                InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu),
        });

        // Send message
        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            "При выборе шаблона, ваши записи на этот жень удалятся!",
            replyMarkup: new InlineKeyboardMarkup(rows));
    }

    public static async Task TemplateTimeTable(BotDbContext context, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, PublicConstants.TemplateTimeTable);

        string property = match.Groups[1].Value;
        string day = match.Groups[2].Value;
        string month = match.Groups[3].Value;
        string year = match.Groups[4].Value;
        string idTemplate = match.Groups[5].Value;

        var template = await context.TimeTableTemplates
            .Include(arg => arg.Template)
            .FirstOrDefaultAsync(arg => arg.Id == long.Parse(idTemplate));

        switch (property)
        {
            // View
            case "0":
                var rows = new InlineKeyboardButton[][] {
                    new[]{
                        InlineKeyboardButton.WithCallbackData("Удалить",$"TR1{day}{month}{year}{idTemplate}"),
                        InlineKeyboardButton.WithCallbackData("Изменить", $"TS{day}{month}{year}{idTemplate}"),
                    },
                    new[]{
                        InlineKeyboardButton.WithCallbackData("Выбрать", $"TR2{day}{month}{year}{idTemplate}"),
                    },
                    PublicConstants.EmptyInlineKeyboardButton,
                    new[]{
                        InlineKeyboardButton.WithCallbackData("Назад", $"TQ{day}{month}{year}"),
                        InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu),
                    }
                };

                var text = new StringBuilder();
                foreach (var item in template.Template)
                {
                    text.AppendLine($"{item.Start.ToString(PublicConstants.TimeFormat)} - {item.IsBusy}");
                }

                // Send message
                await botClient.EditMessageTextAsync(
                    callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId,
                    $"Вы выбрали:\n {text}\nВыбери что сделать:",
                    replyMarkup: new InlineKeyboardMarkup(rows));
                return;

            // Delete
            case "1":
                context.WorkTimes.RemoveRange(template.Template);
                context.TimeTableTemplates.Remove(template);
                await context.SaveChangesAsync();

                callbackQuery.Data = $"TQ{day}{month}{year}";
                await ChooseTemplateTimeTable(context, callbackQuery, botClient);
                return;

            // Input template
            case "2":
                DateOnly currentDate = DateOnly.ParseExact($"{day}/{month}/{year}", PublicConstants.DateFormat, null);
                var dayDelete = context.WorkTimes.Where(arg => arg.UserId == callbackQuery.From.Id && arg.Date == currentDate);
                context.WorkTimes.RemoveRange(dayDelete);

                foreach (var item in template.Template)
                {
                    await context.WorkTimes.AddAsync(new WorkTime
                    {
                        Date = currentDate,
                        Start = item.Start,
                        IsBusy = item.IsBusy,
                        UserId = item.UserId,
                    });
                }
                await context.SaveChangesAsync();

                callbackQuery.Data = $"TG{day}{month}{year}";
                await MenuDayTimeTable(context, callbackQuery, botClient);
                return;
        }
    }

    public static async Task EditTemplateTimeTable(BotDbContext context, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, PublicConstants.EditTemplateTimeTable);

        string day = match.Groups[1].Value;
        string month = match.Groups[2].Value;
        string year = match.Groups[3].Value;
        string idTemplate = match.Groups[4].Value;

        var template = await context.TimeTableTemplates
            .AsNoTracking()
            .Where(arg => arg.Id == long.Parse(idTemplate))
            .Select(arg => arg.Template)
            .FirstOrDefaultAsync();

        // Times
        var rows = new List<InlineKeyboardButton[]>();

        for (var i = 0; i < template.Count;)
        {
            var row = new InlineKeyboardButton[(template.Count - i) >= 4 ? 4 : (template.Count - i) % 4];
            for (var j = 0; j < row.Length; j++)
            {
                row[j] = InlineKeyboardButton.WithCallbackData(
                    $"{template.ElementAt(i).Start.Hour}:{template.ElementAt(i).Start.Minute:00}",
                    $"TT0{day}{month}{year}{template.ElementAt(i).Id}");
                i++;
            }
            rows.Add(row);
        }

        rows.Add(new[] { InlineKeyboardButton.WithCallbackData("Добавить", $"TU{day}{month}{year}{idTemplate}") });
        rows.Add(PublicConstants.EmptyInlineKeyboardButton);
        rows.Add(new[] {
                InlineKeyboardButton.WithCallbackData("Назад", $"TR0{day}{month}{year}{idTemplate}"),
                InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu),
        });

        // Send message
        await botClient.EditMessageTextAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            "Выберите что-нибудь:",
            replyMarkup: new InlineKeyboardMarkup(rows));
    }

    public static async Task EditTimeTemplateTimeTable(BotDbContext context, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, PublicConstants.EditTimeTemplateTimeTable);

        string property = match.Groups[1].Value;
        string day = match.Groups[2].Value;
        string month = match.Groups[3].Value;
        string year = match.Groups[4].Value;
        string idWorkTime = match.Groups[5].Value;

        var workTime = await context.WorkTimes.FirstOrDefaultAsync(arg => arg.Id == long.Parse(idWorkTime));

        switch (property)
        {
            case "1":
                context.WorkTimes.Remove(workTime);
                await context.SaveChangesAsync();
                callbackQuery.Data = $"TS{day}{month}{year}{workTime.TimeTableTemplateId}";
                await EditTemplateTimeTable(context, callbackQuery, botClient);
                return;

            case "2":
                workTime.IsBusy = !workTime.IsBusy;
                await context.SaveChangesAsync();
                goto default;

            default:
                var rows = new InlineKeyboardButton[][]
                {
                    new[] {
                        InlineKeyboardButton.WithCallbackData("Удалить", $"TT1{day}{month}{year}{idWorkTime}"),
                        InlineKeyboardButton.WithCallbackData(workTime.IsBusy ? "Свободно" : "Занято", $"TT2{day}{month}{year}{idWorkTime}"),
                    },
                    PublicConstants.EmptyInlineKeyboardButton,
                    new[] {
                        InlineKeyboardButton.WithCallbackData("Назад", $"TS{day}{month}{year}{workTime.TimeTableTemplateId}"),
                        InlineKeyboardButton.WithCallbackData("Меню", PublicConstants.GoMenu),
                    }
                };

                // Send message
                await botClient.EditMessageTextAsync(
                    callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId,
                    $"Вы выбрали: {workTime.Start.ToString(PublicConstants.TimeFormat)} - {workTime.IsBusy}\nВыберите что-нибудь:",
                    replyMarkup: new InlineKeyboardMarkup(rows));
                return;
        }
    }

    public static async Task SaveNewTimeTemplateTimeTable(BotDbContext context, CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        Match match = Regex.Match(callbackQuery?.Data, PublicConstants.SaveNewTimeTemplateTimeTable);

        string isBusy = match.Groups[1].Value;
        string minute = match.Groups[2].Value;
        string hour = match.Groups[3].Value;
        string day = match.Groups[4].Value;
        string month = match.Groups[5].Value;
        string year = match.Groups[6].Value;
        string idTemplate = match.Groups[7].Value;

        await context.WorkTimes.AddAsync(new WorkTime
        {
            Start = TimeOnly.ParseExact($"{hour}:{minute}", PublicConstants.TimeFormat, null),
            IsBusy = "1" == isBusy,
            UserId = callbackQuery.From.Id,
            TimeTableTemplateId = long.Parse(idTemplate),
        });

        await context.SaveChangesAsync();

        callbackQuery.Data = $"TS{day}{month}{year}{idTemplate}";
        await EditTemplateTimeTable(context, callbackQuery, botClient);
    }
}
