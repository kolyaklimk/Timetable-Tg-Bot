using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TimetableTgBot.Constants;
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
            var userState = await context.GetUserStateAsync(message != null ? message.From : callbackQuery.From);

            if (update.Type == UpdateType.Message)
            {

                // start
                if (message.Text == "/start")
                {
                    var user = await context.GetUserAsync(message.From);

                    if (user == null)
                    {
                        await context.RegisterUserAsync(message);
                    }
                    else
                    {
                        context.UpdateUserStateAsync(userState, false);
                        await context.SetNullUserBuffer3(message.From);
                    }

                    await GeneralCommands.DeleteMessage(botClient, message, true);
                    await GeneralCommands.CreateMenu(false, botClient, message);

                    await context.SaveChangesAsync();
                    return;
                }

                // Check WaitingForText
                if (userState.WaitingForText)
                {
                    var userBuffer = await context.GetUserBufferAsync(message.From);
                    await GeneralCommands.DeleteMessage(botClient, message);

                    switch (userBuffer.Buffer1[1])
                    {
                        // Description in create
                        case 'E':
                            userBuffer.Buffer3 = message.Text;
                            await TimeTableCommands.AddDescriptionTimeTable(message.Text, userBuffer.Buffer1, botClient, message.Chat, (int)userBuffer.Buffer2);
                            break;

                        // Description in edit time
                        case 'K':
                            if (userBuffer.Buffer1[2] == 'D')
                            {
                                context.UpdateUserStateAsync(userState, false);
                            }
                            else
                            {
                                userBuffer.Buffer1 = $"TK0{userBuffer.Buffer1[3..]}";
                                await TimeTableCommands.EditTimeTimeTable(message.Text, userBuffer.Buffer1, context, botClient, message.Chat, (int)userBuffer.Buffer2);
                            }
                            break;
                    }
                    await context.SaveChangesAsync();
                    return;
                }
            }

            if (update.Type == UpdateType.CallbackQuery)
            {
                Console.WriteLine(callbackQuery.Data);
                // Check WaitingForText
                if (userState.WaitingForText)
                {
                    context.UpdateUserStateAsync(userState, false);
                    if (callbackQuery?.Data[1] != 'F')
                        await context.SetNullUserBuffer3(callbackQuery.From);
                    await context.SaveChangesAsync();
                }

                switch (callbackQuery?.Data[0])
                {
                    // Go main Menu
                    case 'M':
                        await GeneralCommands.CreateMenu(true, botClient, callbackQuery.Message);
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
                                await TimeTableCommands.ChooseHourTimeTable('C', 'G', callbackQuery, botClient);
                                return;

                            // Choose minute
                            case 'C':
                                await TimeTableCommands.ChooseMinuteTimeTable('D', 'B', callbackQuery, botClient);
                                return;

                            // Choose is busy
                            case 'D':
                                await TimeTableCommands.ChooseIsBusyTimeTable("EN", 'C', callbackQuery, botClient);
                                return;

                            // Add description
                            case 'E':
                                var userBuffer = await context.GetUserBufferAsync(callbackQuery.From);
                                context.UpdateUserStateAsync(userState, true);
                                await context.UpdateUserBuffer_1_2_Async(callbackQuery);
                                if (callbackQuery?.Data[2] == 'Y')
                                    userBuffer.Buffer3 = null;

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
                                context.UpdateUserStateAsync(userState, true);
                                await context.UpdateUserBuffer_1_2_Async(callbackQuery);

                                await TimeTableCommands.EditTimeTimeTable(null, callbackQuery?.Data, context, botClient, callbackQuery.Message.Chat, callbackQuery.Message.MessageId, callbackQuery);
                                return;

                            // Create template -> Choose hour
                            case 'M':
                                await TimeTableCommands.ChooseHourTimeTable('N', 'Q', callbackQuery, botClient);
                                return;

                            // Create template -> Choose minute
                            case 'N':
                                await TimeTableCommands.ChooseMinuteTimeTable('O', 'M', callbackQuery, botClient);
                                return;

                            // Create template -> Choose isBusy
                            case 'O':
                                await TimeTableCommands.ChooseIsBusyTimeTable("P", 'N', callbackQuery, botClient);
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
                                await TimeTableCommands.ChooseHourTimeTable('V', 'S', callbackQuery, botClient);
                                return;

                            // Create new time in template -> Choose minute
                            case 'V':
                                await TimeTableCommands.ChooseMinuteTimeTable('W', 'U', callbackQuery, botClient);
                                return;

                            // Create new time in template -> Choose isBusy
                            case 'W':
                                await TimeTableCommands.ChooseIsBusyTimeTable("X", 'V', callbackQuery, botClient);
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

                            // Choose day 
                            case 'A':
                                await GeneralCommands.ChooseDay("IG", "IM", callbackQuery, botClient);
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
