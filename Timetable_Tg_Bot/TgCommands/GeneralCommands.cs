﻿using Telegram.Bot;
using Telegram.Bot.Types;

namespace TimetableTgBot.TgCommands;

public static class GeneralCommands
{
    public static async Task DeleteMessage(ITelegramBotClient botClient, Message message, bool previous = false)
    {
        if (previous)
        {
            int messageId = message.MessageId;
            while (true)
            {
                try
                {
                    await botClient.DeleteMessageAsync(message.Chat.Id, messageId--);
                }
                catch
                {
                    break;
                }
            }
        }
        else
        {
            await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
        }
    }

    public static async Task CreateMenu(bool edit, ITelegramBotClient botClient, Message message)
    {
        if (edit)
        {
            await botClient.EditMessageTextAsync(
                message.Chat.Id,
                message.MessageId,
                "Меню:",
                replyMarkup: Constants.MenuMarkup);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Меню:",
                replyMarkup: Constants.MenuMarkup);
        }
    }
}
