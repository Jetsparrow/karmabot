using JetKarmaBot.Commands;
using JetKarmaBot.Models;
using JetKarmaBot.Services;
using Perfusion;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetKarmaBot
{
    public class JetKarmaBot : IDisposable
    {
        [Inject] Config Config { get; set; }
        [Inject] Container Container { get; set; }
        [Inject] KarmaContextFactory Db { get; set; }

        TelegramBotClient Client { get; set; }
        ChatCommandRouter Commands;
        Telegram.Bot.Types.User Me { get; set; }

        public async Task Init()
        {
            var httpProxy = new WebProxy($"{Config.Proxy.Url}:{Config.Proxy.Port}")
            {
                Credentials = new NetworkCredential(Config.Proxy.Login, Config.Proxy.Password)
            };

            Client = new TelegramBotClient(Config.ApiKey, httpProxy);
            Container.AddInstance(Client);
            Me = await Client.GetMeAsync();

            InitCommands(Container);

            Client.OnMessage += BotOnMessageReceived;
            Client.StartReceiving();
        }

        public async Task Stop()
        {
            Dispose();
        }

        #region service

        void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text)
                return;
            using (KarmaContext db = Db.GetContext())
            {
                if (!db.Users.Any(x => x.UserId == messageEventArgs.Message.From.Id))
                    db.Users.Add(new Models.User { UserId = messageEventArgs.Message.From.Id });
                if (messageEventArgs.Message.ReplyToMessage != null)
                    if (!db.Users.Any(x => x.UserId == messageEventArgs.Message.ReplyToMessage.From.Id))
                        db.Users.Add(new Models.User { UserId = messageEventArgs.Message.ReplyToMessage.From.Id });
                if (!db.Chats.Any(x => x.ChatId == messageEventArgs.Message.Chat.Id))
                    db.Chats.Add(new Models.Chat { ChatId = messageEventArgs.Message.Chat.Id });
                db.SaveChanges();
            }
            string s = message.Text;
            long id = message.Chat.Id;
            long from = message.From.Id;
            Task.Run(() => Commands.Execute(sender, messageEventArgs));
        }

        void InitCommands(Container c)
        {
            Commands = c.ResolveObject(new ChatCommandRouter(Me));
            Commands.Add(c.ResolveObject(new HelpCommand(Commands)));
            Commands.Add(c.ResolveObject(new AwardCommand(Me)));
            Commands.Add(c.ResolveObject(new StatusCommand()));
            Commands.Add(c.ResolveObject(new LocaleCommand()));
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Client.StopReceiving();
        }

        #endregion
    }
}
