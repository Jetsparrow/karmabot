using JetKarmaBot.Commands;
using Perfusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetKarmaBot
{
    public class JetKarmaBot : IDisposable
    {
        public void Broadcast(string message)
        {
            foreach (var u in db.Chats)
                client.SendTextMessageAsync(u.Value.ChatId, message);
        }

        public JetKarmaBot([Inject(true)]Config cfg)
        {
            var httpProxy = new WebProxy($"{cfg.ProxyUrl}:{cfg.ProxyPort}")
            {
                Credentials = new NetworkCredential(cfg.ProxyLogin, cfg.ProxyPassword)
            };
            var botClient = new TelegramBotClient(cfg.ApiKey, httpProxy);
            var cred = new NetworkCredential(cfg.ProxyLogin, cfg.ProxyPassword);
            client = new TelegramBotClient(cfg.ApiKey, httpProxy);
            me = client.GetMeAsync().Result;
            InitCommands();
            client.OnMessage += BotOnMessageReceived;
            client.StartReceiving();
        }

        #region IDisposable
        public void Dispose()
        {
            client.StopReceiving();
        }
        #endregion

        #region service
        [Inject(true)] Db db { get; set; }
        TelegramBotClient client { get; }
        [Inject(true)] Container container { get; set; }
        User me { get; }

        ChatCommandRouter commands;
        void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text)
                return;

            string s = message.Text;
            long id = message.Chat.Id;
            long from = message.From.Id;
            Task.Run(() => commands.Execute(sender, messageEventArgs));
        }
        void InitCommands()
        {
            commands = new ChatCommandRouter();
            commands.Add(container.ResolveObject(new StartCommand(db)));
            commands.Add(container.ResolveObject(new EchoCommand(client)));
            commands.Add(container.ResolveObject(new DefineCommand(client)));
            commands.Add(container.ResolveObject(new AwardCommand(db, client, me)));
        }

        #endregion
    }
}
