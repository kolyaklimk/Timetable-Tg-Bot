﻿using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
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

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;
        var callbackQuery = update.CallbackQuery;

        #region /start

        if (message?.Text == "/start")
        {
            var user = await DbContext.Users.FirstOrDefaultAsync(arg => arg.Id == message.From.Id,cancellationToken);

            if (user != null && !string.IsNullOrEmpty(user.Name) && !string.IsNullOrEmpty(user.Password))
            {
                await StartCommands.LogInAndRegister(botClient, message, callbackQuery);

                await GeneralCommands.DeleteMessage(botClient, message);
            }
        }

        #endregion
    }

    Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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