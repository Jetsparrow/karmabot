using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetKarmaBot.Commands;
using JetKarmaBot.Models;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace JetKarmaBot.Services.Handling
{
    public class RequestContext : IServiceProvider
    {
        public ITelegramBotClient Client { get; }
        public Update EventArgs { get; }
        public CommandString Command { get; }
        public Dictionary<Type, object> Features { get; } = new Dictionary<Type, object>();
        public RequestContext(ITelegramBotClient client, Update args, CommandString cmd)
        {
            Client = client;
            EventArgs = args;
            Command = cmd;
        }
        public object GetService(Type serviceType) => Features[serviceType];
        public T GetFeature<T>() => (T)Features[typeof(T)];
        public void AddFeature<T>(T feat) => Features[typeof(T)] = feat;

        //Method to reduce WET in commands
        public Task SendMessage(string text) 
            => Client.SendMessage(
                chatId: EventArgs.Message.Chat.Id,
                text: text,
                disableNotification: true,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                replyParameters: new ReplyParameters { MessageId = EventArgs.Message.MessageId }
            );
    }
}