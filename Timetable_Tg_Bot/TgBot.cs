using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
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

        using CancellationTokenSource cts = new();

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
        };

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );
    }

    async private Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;
            var waitingForText = await DbContext.UserState.FirstOrDefaultAsync(arg => arg.User.Id == message.From.Id, cancellationToken);

            if (update.Type == UpdateType.Message)
            {
                // start
                if (message.Text == "/start")
                {
                    var user = await DbContext.Users.FirstOrDefaultAsync(arg => arg.Id == message.From.Id, cancellationToken);

                    if (user == null)
                    {
                        var qwe = await DbContext.Users.AddAsync(new Entities.User
                        {
                            Id = message.From.Id,
                            FirstName = message.From.FirstName,
                            LastName = message.From.LastName,
                            UserName = message.From.Username,
                            Subscription = DateTime.Now.AddDays(3)
                        }, cancellationToken);

                        await DbContext.UserState.AddAsync(new UserState
                        {
                            User = qwe.Entity
                        }, cancellationToken);

                        await DbContext.SaveChangesAsync(cancellationToken);
                    }

                    await GeneralCommands.DeleteMessage(botClient, message, cancellationToken);
                    await GeneralCommands.CreateMenu(false, botClient, message, cancellationToken);
                    return;
                }

                if (waitingForText.WaitingForText)
                {
                    waitingForText.WaitingForText = false;
                    await DbContext.SaveChangesAsync();
                    Match match = Regex.Match(callbackQuery?.Data, Constants.SaveTimeTable);

                    await GeneralCommands.DeleteMessage(botClient, message, cancellationToken);
                    await TimeTableCommands.SaveTimeTable(match, botClient, callbackQuery, cancellationToken);
                    return;
                }
            }

            if (update.Type == UpdateType.CallbackQuery)
            {
                #region GoMenu
                if (callbackQuery?.Data == Constants.GoMenu)
                {
                    await GeneralCommands.CreateMenu(true, botClient, callbackQuery.Message, cancellationToken);
                    return;
                }
                #endregion

                if (callbackQuery?.Data[0] == 'T')
                {
                    // Menu TimeTable
                    if (callbackQuery?.Data == Constants.MenuTimeTable)
                    {

                        await TimeTableCommands.MenuTimeTable(botClient, callbackQuery.Message, cancellationToken);
                        return;
                    }

                    // Choose day
                    if (Regex.IsMatch(callbackQuery?.Data, Constants.ChooseMonthTimeTable))
                    {
                        Match match = Regex.Match(callbackQuery?.Data, Constants.ChooseMonthTimeTable);

                        await TimeTableCommands.ChooseDateTimeTable(match, botClient, callbackQuery.Message, cancellationToken);
                        return;
                    }

                    // Menu Day
                    if (Regex.IsMatch(callbackQuery?.Data, Constants.MenuDayTimeTable))
                    {
                        Match match = Regex.Match(callbackQuery?.Data, Constants.MenuDayTimeTable);

                        await TimeTableCommands.MenuDayTimeTable(match, botClient, callbackQuery.Message, cancellationToken);
                        return;
                    }

                    // Choose hour
                    if (Regex.IsMatch(callbackQuery?.Data, Constants.ChooseHourTimeTable))
                    {
                        Match match = Regex.Match(callbackQuery?.Data, Constants.ChooseHourTimeTable);

                        await TimeTableCommands.ChooseHourTimeTable(match, botClient, callbackQuery.Message, cancellationToken);
                        return;
                    }

                    // Choose minute
                    if (Regex.IsMatch(callbackQuery?.Data, Constants.ChooseMinuteTimeTable))
                    {
                        Match match = Regex.Match(callbackQuery?.Data, Constants.ChooseMinuteTimeTable);

                        await TimeTableCommands.ChooseMinuteTimeTable(match, botClient, callbackQuery.Message, cancellationToken);
                        return;
                    }

                    // Choose is busy
                    if (Regex.IsMatch(callbackQuery?.Data, Constants.ChooseIsBusyTimeTable))
                    {
                        Match match = Regex.Match(callbackQuery?.Data, Constants.ChooseIsBusyTimeTable);

                        await TimeTableCommands.ChooseIsBusyTimeTable(match, botClient, callbackQuery.Message, cancellationToken);
                        return;
                    }

                    // Add description
                    if (Regex.IsMatch(callbackQuery?.Data, Constants.AddDescriptionTimeTable))
                    {
                        Match match = Regex.Match(callbackQuery?.Data, Constants.AddDescriptionTimeTable);

                        await TimeTableCommands.AddDescriptionTimeTable(match, botClient, callbackQuery.Message, cancellationToken);
                        return;
                    }

                    // Save TimeTable
                    if (Regex.IsMatch(callbackQuery?.Data, Constants.SaveTimeTable))
                    {
                        Match match = Regex.Match(callbackQuery?.Data, Constants.SaveTimeTable);

                        await TimeTableCommands.SaveTimeTable(match, botClient, callbackQuery, cancellationToken);
                        return;
                    }
                }

                #region ImageMenu
                if (callbackQuery?.Data == Constants.ImageMenu)
                {

                    //await GeneralCommands.DeleteMessage(botClient, message, cancellationToken);
                    return;
                }
                #endregion

                #region SupportMenu
                if (callbackQuery?.Data == Constants.SupportMenu)
                {

                    //await GeneralCommands.DeleteMessage(botClient, message, cancellationToken);
                    return;
                }
                #endregion

                #region SubscribeMenu
                if (callbackQuery?.Data == Constants.SubscribeMenu)
                {

                    //await GeneralCommands.DeleteMessage(botClient, message, cancellationToken);
                    return;
                }
                #endregion

                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
            }

            else
            {
                await GeneralCommands.DeleteMessage(botClient, message, cancellationToken);
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
