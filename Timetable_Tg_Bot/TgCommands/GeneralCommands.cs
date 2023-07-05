using Telegram.Bot;
using Telegram.Bot.Types;

namespace TimetableTgBot.TgCommands;

public static class GeneralCommands
{
    public static async Task DeleteMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken);
        }
        catch
        {
            Console.WriteLine("DeleteMessageAsync error in /start");
        }
    }
}
