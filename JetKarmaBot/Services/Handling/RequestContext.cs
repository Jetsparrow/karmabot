using System;
using System.Collections.Generic;
using System.Linq;
using JetKarmaBot.Commands;
using JetKarmaBot.Models;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace JetKarmaBot.Services.Handling
{
    public class RequestContext : IServiceProvider
    {
        public ITelegramBotClient Client { get; }
        public MessageEventArgs EventArgs { get; }
        public CommandString Command { get; }
        public KarmaContext Database { get; }
        public ICollection<object> Features { get; } = new List<object>();
        public RequestContext(ITelegramBotClient client, MessageEventArgs args, CommandString cmd, KarmaContext db)
        {
            Client = client;
            EventArgs = args;
            Command = cmd;
            Database = db;
        }
        public object GetService(Type serviceType) => Features.First(x => x.GetType() == serviceType);
        public T GetFeature<T>() => (T)Features.First(x => x is T);
    }
}