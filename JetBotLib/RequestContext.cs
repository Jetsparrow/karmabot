using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace JetBotLib
{
    public class RequestContext : IServiceProvider
    {
        public ITelegramBotClient Client { get; }
        public MessageEventArgs EventArgs { get; }
        public CommandString Command { get; }
        public ICollection<object> Features { get; } = new List<object>();
        public RequestContext(ITelegramBotClient client, MessageEventArgs args, CommandString cmd)
        {
            Client = client;
            EventArgs = args;
            Command = cmd;
        }
        public object GetService(Type serviceType) => Features.First(x => x.GetType() == serviceType);
        public T GetFeature<T>() => (T)Features.First(x => x is T);

        //Method to reduce WET in commands
        public Task SendMessage(string text) => Client.SendTextMessageAsync(
                                                    EventArgs.Message.Chat.Id,
                                                    text,
                                                    replyToMessageId: EventArgs.Message.MessageId,
                                                    disableNotification: true,
                                                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
    }
}