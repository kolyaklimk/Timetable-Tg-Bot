using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TimetableTgBot.Entities;
using TimetableTgBot.TgCommands;

namespace TimetableTgBot;

public class TgBot
{
    private readonly BotDbContext DbContext = new BotDbContext();
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
            var waitingForText = await DbContext.UserState.FirstOrDefaultAsync(
                arg => arg.User.Id == (message != null ? message.From.Id : callbackQuery.From.Id));


            if (update.Type == UpdateType.Message)
            {
                // start
                if (message.Text == "/start")
                {
                    var user = await DbContext.Users.FirstOrDefaultAsync(arg => arg.Id == message.From.Id);

                    if (user == null)
                    {
                        var qwe = await DbContext.Users.AddAsync(new Entities.User
                        {
                            Id = message.From.Id,
                            FirstName = message.From.FirstName,
                            LastName = message.From.LastName,
                            UserName = message.From.Username,
                            Subscription = DateTime.Now.AddDays(3)
                        });

                        await DbContext.UserState.AddAsync(new UserState { User = qwe.Entity });

                        await DbContext.SaveChangesAsync();
                    }

                    await GeneralCommands.DeleteMessage(botClient, message);
                    await GeneralCommands.CreateMenu(false, botClient, message);
                    return;
                }

                if (waitingForText.WaitingForText)
                {
                    await GeneralCommands.DeleteMessage(botClient, message);
                    await TimeTableCommands.AddDescriptionTimeTable(message.Text, waitingForText.Buffer1, botClient, message.Chat, (int)waitingForText.Buffer2);
                    return;
                }
            }

            if (update.Type == UpdateType.CallbackQuery)
            {
                // Check WaitingForText
                if (waitingForText.WaitingForText)
                {
                    waitingForText.WaitingForText = false;
                    await DbContext.SaveChangesAsync();
                }

                // Go main Menu
                if (callbackQuery?.Data == Constants.GoMenu)
                {
                    await GeneralCommands.CreateMenu(true, botClient, callbackQuery.Message);
                    return;
                }

                if (callbackQuery?.Data[0] == 'T')
                {
                    // Menu TimeTable
                    if (callbackQuery?.Data[1] == 'H')
                    {
                        await TimeTableCommands.MenuTimeTable(botClient, callbackQuery.Message);
                        return;
                    }

                    // Choose Date
                    if (callbackQuery?.Data[1] == 'A')
                    {
                        await TimeTableCommands.ChooseDateTimeTable(callbackQuery, botClient);
                        return;
                    }

                    // Menu Day
                    if (callbackQuery?.Data[1] == 'G')
                    {
                        await TimeTableCommands.MenuDayTimeTable(callbackQuery, botClient);
                        return;
                    }

                    // Choose hour
                    if (callbackQuery?.Data[1] == 'B')
                    {
                        await TimeTableCommands.ChooseHourTimeTable(callbackQuery, botClient);
                        return;
                    }

                    // Choose minute
                    if (callbackQuery?.Data[1] == 'C')
                    {
                        await TimeTableCommands.ChooseMinuteTimeTable(callbackQuery, botClient);
                        return;
                    }

                    // Choose is busy
                    if (callbackQuery?.Data[1] == 'D')
                    {
                        await TimeTableCommands.ChooseIsBusyTimeTable(callbackQuery, botClient);
                        return;
                    }

                    // Add description
                    if (callbackQuery?.Data[1] == 'E')
                    {
                        await TimeTableCommands.AddDescriptionTimeTable(null, callbackQuery?.Data, botClient, callbackQuery.Message.Chat, callbackQuery.Message.MessageId);

                        waitingForText.WaitingForText = true;
                        waitingForText.Buffer1 = callbackQuery.Data;
                        waitingForText.Buffer2 = callbackQuery.Message.MessageId;
                        await DbContext.SaveChangesAsync();
                        return;
                    }

                    // Save TimeTable
                    if (callbackQuery?.Data[1] == 'F')
                    {
                        await TimeTableCommands.SaveTimeTable(callbackQuery, botClient);

                        waitingForText.WaitingForText = false;
                        await DbContext.SaveChangesAsync();
                        return;
                    }
                }

                #region ImageMenu
                if (callbackQuery?.Data == Constants.ImageMenu)
                {

                    //await GeneralCommands.DeleteMessage(botClient, message);
                    return;
                }
                #endregion

                #region SupportMenu
                if (callbackQuery?.Data == Constants.SupportMenu)
                {

                    //await GeneralCommands.DeleteMessage(botClient, message);
                    return;
                }
                #endregion

                #region SubscribeMenu
                if (callbackQuery?.Data == Constants.SubscribeMenu)
                {

                    //await GeneralCommands.DeleteMessage(botClient, message);
                    return;
                }
                #endregion

                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            }

            else
            {
                await GeneralCommands.DeleteMessage(botClient, message);
                return;
            }
        }
        catch (ApiRequestException apiRequestException)
        {
            Console.WriteLine(update.Message?.From?.FirstName + " " + update.Message?.From?.Username + " " + DateTime.Now);
            Console.WriteLine($"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine(update.Message?.From?.FirstName + " " + update.Message?.From?.Username + " " + DateTime.Now);
            Console.WriteLine($"{ex}\n");
        }
        catch
        {
            Console.WriteLine(update.Message?.From?.FirstName + " " + update.Message?.From?.Username + " " + DateTime.Now + " \nError\n");
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
