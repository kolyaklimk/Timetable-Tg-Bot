using System.Security.AccessControl;
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

            using (var context = new BotDbContext())
            {
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
                        }

                        await GeneralCommands.DeleteMessage(botClient, message, true);
                        await GeneralCommands.CreateMenu(false, botClient, message);

                        await context.SaveChangesAsync();
                        return;
                    }

                    if (userState.WaitingForText)
                    {
                        var userBuffer = await context.GetUserBufferAsync(message.From);
                        userBuffer.Buffer3 = message.Text;

                        await GeneralCommands.DeleteMessage(botClient, message);
                        await TimeTableCommands.AddDescriptionTimeTable(message.Text, userBuffer.Buffer1, botClient, message.Chat, (int)userBuffer.Buffer2);

                        await context.SaveChangesAsync();
                        return;
                    }
                }

                if (update.Type == UpdateType.CallbackQuery)
                {
                    // Check WaitingForText
                    if (userState.WaitingForText)
                    {
                        context.UpdateUserStateAsync(userState, false);
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
                                // Menu TimeTable
                                case 'H':
                                    await TimeTableCommands.MenuTimeTable(botClient, callbackQuery.Message);
                                    return;

                                // Choose Date
                                case 'A':
                                    await TimeTableCommands.ChooseDateTimeTable(callbackQuery, botClient);
                                    return;

                                // Menu Day
                                case 'G':
                                    await TimeTableCommands.MenuDayTimeTable(context, callbackQuery, botClient);
                                    return;

                                // Choose hour
                                case 'B':
                                    await TimeTableCommands.ChooseHourTimeTable(callbackQuery, botClient);
                                    return;

                                // Choose minute
                                case 'C':
                                    await TimeTableCommands.ChooseMinuteTimeTable(callbackQuery, botClient);
                                    return;

                                // Choose is busy
                                case 'D':
                                    await TimeTableCommands.ChooseIsBusyTimeTable(callbackQuery, botClient);
                                    return;

                                // Add description
                                case 'E':
                                    await TimeTableCommands.AddDescriptionTimeTable(null, callbackQuery?.Data, botClient, callbackQuery.Message.Chat, callbackQuery.Message.MessageId);

                                    context.UpdateUserStateAsync(userState, true);
                                    await context.UpdateUserBuffer_1_2_Async(callbackQuery);
                                    await context.SaveChangesAsync();
                                    return;

                                // Save TimeTable
                                case 'F':
                                    context.UpdateUserStateAsync(userState, false);
                                    await TimeTableCommands.SaveTimeTable(context, callbackQuery, botClient);
                                    return;

                                // Save TimeTable
                                case 'I':
                                    await TimeTableCommands.DeleteDayTimeTable(context, callbackQuery, botClient);
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
