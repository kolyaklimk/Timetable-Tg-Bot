using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TimetableTgBot.Constants;
using TimetableTgBot.Entities;
using TimetableTgBot.Services;
using TimetableTgBot.TgCommands;

namespace TimetableTgBot;

public class TgBot
{
    public TgBot()
    {
        var botClient = new TelegramBotClient(PrivateConstants.TOKEN_TG_BOT);

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
        };

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions
        );
    }

    async private Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;

            using var context = new BotDbContext();
            var waitingFor = await context.GetUserStateAsync(message != null ? message.From : callbackQuery.From);

            //Console.WriteLine(context.ChangeTracker.Entries().Count()); // проверка кол-во отслеживающих объектов

            if (update.Type == UpdateType.Message)
            {
                // start
                if (message.Text == "/start")
                {
                    if (!await context.UserExistsAsync(message.From))
                    {
                        await context.RegisterUserAsync(message);
                    }
                    else
                    {
                        context.UpdateUserStateTextAsync(message.From, false);
                        context.UpdateUserBuffer3(message.From, string.Empty);
                    }
                    await context.SaveChangesAsync();

                    await GeneralCommands.DeleteMessage(botClient, message, true);
                    await GeneralCommands.CreateMenu(context, false, botClient, message);
                    return;
                }

                // Check WaitingForText
                if (waitingFor.WaitingForText)
                {
                    var userBuffer = await context.GetUserBuffersAsync(message.From, arg => new UserBuffer { Buffer1 = arg.Buffer1, MessageId = arg.MessageId });
                    await GeneralCommands.DeleteMessage(botClient, message);

                    switch (userBuffer.Buffer1[1])
                    {
                        // Description in create
                        case 'E':
                            context.UpdateUserBuffer3(message.From, message.Text);
                            await TimeTableCommands.AddDescriptionTimeTable(message.Text, userBuffer.Buffer1, botClient, message.Chat, (int)userBuffer.MessageId);
                            break;

                        // Description in edit time
                        case 'K':
                            if (userBuffer.Buffer1[2] == 'D')
                            {
                                context.UpdateUserStateTextAsync(message.From, false);
                                break;
                            }
                            else
                            {
                                var newBuf = $"TK0{userBuffer.Buffer1[3..]}";
                                context.UpdateUserBuffer1(message.From, newBuf);
                                await TimeTableCommands.EditTimeTimeTable(message.Text, newBuf, context, botClient, message.Chat, (int)userBuffer.MessageId);
                            }
                            return;
                    }
                    await context.SaveChangesAsync();
                    return;
                }

                if (waitingFor.WaitingForDocument)
                {
                    var userBuffer = await context.GetUserBuffersAsync(message.From, arg => new UserBuffer { Buffer1 = arg.Buffer1, MessageId = arg.MessageId });

                    if (message.Type == MessageType.Document)
                    {
                        if (message.Document.MimeType.StartsWith("image/"))
                        {
                            if (message.Document.FileSize < 11_000_000)
                            {
                                await ImageGeneration.CreateTimeTableV1(message.From, context);
                            }
                            else
                            {
                                await ImageCommands.LoadUserImage(userBuffer.Buffer1, botClient, message.Chat, (int)userBuffer.MessageId, $"Ошибка: файл больше 10 Мб\\!");
                            }
                        }
                        else
                        {
                            await ImageCommands.LoadUserImage(userBuffer.Buffer1, botClient, message.Chat, (int)userBuffer.MessageId, $"Ошибка: Отправте фото  Документом\\!");
                        }
                    }
                    else
                    {
                        await ImageCommands.LoadUserImage(userBuffer.Buffer1, botClient, message.Chat, (int)userBuffer.MessageId, $"Ошибка: Отправте фото Документом\\!");
                    }
                }
            }

            if (update.Type == UpdateType.CallbackQuery)
            {
                Console.WriteLine(callbackQuery.Data);
                // Check WaitingForText
                if (waitingFor.WaitingForText)
                {
                    context.UpdateUserStateTextAsync(callbackQuery.From, false);
                    if (callbackQuery?.Data[1] != 'F')
                        context.UpdateUserBuffer3(callbackQuery.From, string.Empty);
                    await context.SaveChangesAsync();
                }

                switch (callbackQuery?.Data[0])
                {
                    // Go main Menu
                    case 'M':
                        await GeneralCommands.CreateMenu(context, true, botClient, callbackQuery.Message);
                        return;

                    // Timetable
                    case 'T':
                        switch (callbackQuery?.Data[1])
                        {
                            // Choose Date
                            case 'A':
                                await GeneralCommands.ChooseDay("TG", null, callbackQuery, botClient);
                                return;

                            // Menu Day
                            case 'G':
                                await TimeTableCommands.MenuDayTimeTable(context, callbackQuery, botClient);
                                return;

                            // Choose hour
                            case 'B':
                                await TimeTableCommands.ChooseHourTimeTable("C", "G", callbackQuery, botClient);
                                return;

                            // Choose minute
                            case 'C':
                                await TimeTableCommands.ChooseMinuteTimeTable("D", "B", callbackQuery, botClient);
                                return;

                            // Choose is busy
                            case 'D':
                                await TimeTableCommands.ChooseIsBusyTimeTable("EN", "C", callbackQuery, botClient);
                                return;

                            // Add description
                            case 'E':
                                var userBuffer = await context.GetUserBuffersAsync(callbackQuery.From, arg => new UserBuffer { Buffer3 = arg.Buffer3 });
                                context.UpdateUserStateTextAsync(callbackQuery.From, true);
                                context.UpdateUserBuffer1(callbackQuery.From, callbackQuery.Data);
                                if (callbackQuery?.Data[2] == 'Y')
                                {
                                    context.UpdateUserBuffer3(callbackQuery.From, string.Empty);
                                    userBuffer.Buffer3 = null;
                                }

                                await TimeTableCommands.AddDescriptionTimeTable(userBuffer.Buffer3, callbackQuery?.Data, botClient, callbackQuery.Message.Chat, callbackQuery.Message.MessageId);
                                await context.SaveChangesAsync();
                                return;

                            // Save
                            case 'F':
                                await TimeTableCommands.SaveTimeTable(context, callbackQuery, botClient);
                                return;

                            // Delete day
                            case 'I':
                                await TimeTableCommands.DeleteDayTimeTable(context, callbackQuery, botClient);
                                return;

                            // Choose time for edit
                            case 'J':
                                await TimeTableCommands.ChooseTimeTimeTable(context, callbackQuery, botClient);
                                return;

                            // Edit time
                            case 'K':
                                context.UpdateUserStateTextAsync(callbackQuery.From, true);
                                context.UpdateUserBuffer1(callbackQuery.From, callbackQuery.Data);

                                await TimeTableCommands.EditTimeTimeTable(null, callbackQuery?.Data, context, botClient, callbackQuery.Message.Chat, callbackQuery.Message.MessageId, callbackQuery);
                                return;

                            // Create template -> Choose hour
                            case 'M':
                                await TimeTableCommands.ChooseHourTimeTable("N", "Q", callbackQuery, botClient);
                                return;

                            // Create template -> Choose minute
                            case 'N':
                                await TimeTableCommands.ChooseMinuteTimeTable("O", "M", callbackQuery, botClient);
                                return;

                            // Create template -> Choose isBusy
                            case 'O':
                                await TimeTableCommands.ChooseIsBusyTimeTable("P", "N", callbackQuery, botClient);
                                return;

                            // Create new template
                            case 'P':
                                await TimeTableCommands.CreateNewTemplateTimeTable(context, callbackQuery, botClient);
                                return;

                            // Choose template
                            case 'Q':
                                await TimeTableCommands.ChooseTemplateTimeTable(context, callbackQuery, botClient);
                                return;

                            // Work with template
                            case 'R':
                                await TimeTableCommands.TemplateTimeTable(context, callbackQuery, botClient);
                                return;

                            // Edit template
                            case 'S':
                                await TimeTableCommands.EditTemplateTimeTable(context, callbackQuery, botClient);
                                return;

                            // Edit time template
                            case 'T':
                                await TimeTableCommands.EditTimeTemplateTimeTable(context, callbackQuery, botClient);
                                return;

                            // Create new time in template -> Choose hour
                            case 'U':
                                await TimeTableCommands.ChooseHourTimeTable("V", "S", callbackQuery, botClient);
                                return;

                            // Create new time in template -> Choose minute
                            case 'V':
                                await TimeTableCommands.ChooseMinuteTimeTable("W", "U", callbackQuery, botClient);
                                return;

                            // Create new time in template -> Choose isBusy
                            case 'W':
                                await TimeTableCommands.ChooseIsBusyTimeTable("X", "V", callbackQuery, botClient);
                                return;

                            // Create new time in template -> Save new time
                            case 'X':
                                await TimeTableCommands.SaveNewTimeTemplateTimeTable(context, callbackQuery, botClient);
                                return;
                        }
                        return;

                    case 'I':
                        switch (callbackQuery.Data[1])
                        {
                            // Image menu
                            case 'M':
                                await ImageCommands.ImageMenu(callbackQuery, botClient);
                                return;

                            // Choose first day 
                            case 'A':
                                await GeneralCommands.ChooseDay("IB", "IM", callbackQuery, botClient);
                                return;

                            // Choose second day 
                            case 'B':
                                if (callbackQuery.Data.Length < 11)
                                {
                                    callbackQuery.Data = $"IB{callbackQuery.Data[4..]}{callbackQuery.Data[2..]}";
                                }
                                await GeneralCommands.ChooseDay("IR1", $"IA{callbackQuery.Data[2..8]}", callbackQuery, botClient);
                                return;

                            // Range of days
                            case 'R':
                                await ImageCommands.RangeOfDaysImage(context, callbackQuery, botClient);
                                return;

                            // Choose template
                            case 'C':
                                await ImageCommands.ChooseTemplateImage(context, callbackQuery, botClient);
                                return;

                            // Edit template
                            case 'H':
                                await ImageCommands.EditTemplateImage(callbackQuery, botClient);
                                return;

                            // Load User Image
                            case 'L':
                                context.UpdateUserStateDocumentAsync(callbackQuery.From, true);
                                context.UpdateUserBuffer1(callbackQuery.From, callbackQuery.Data);
                                await context.SaveChangesAsync();

                                await ImageCommands.LoadUserImage(callbackQuery.Data, botClient, callbackQuery.Message.Chat, callbackQuery.Message.MessageId);
                                return;

                            // Create Image
                            case 'P':
                                await ImageCommands.CreateImage(context, callbackQuery, botClient);
                                return;
                        }
                        return;
                    // Answer other null callback
                    default:
                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                        return;
                }
            }

            await GeneralCommands.DeleteMessage(botClient, message);
        }
        catch (ApiRequestException apiRequestException)
        {
            Console.WriteLine($"{update.Message?.From?.FirstName} {update.Message?.From?.Username} {DateTime.Now}\n");
            Console.WriteLine($"Telegram API Error: [{apiRequestException.ErrorCode}] - {apiRequestException.Message}\n");

            try { await GeneralCommands.DeleteMessage(botClient, update.Message); } catch { }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{update.Message?.From?.FirstName} {update.Message?.From?.Username} {DateTime.Now}\n{ex}\n");
        }
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}
