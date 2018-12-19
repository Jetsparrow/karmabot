using JetKarmaBot.Commands;
using Perfusion;
using System;
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
        [Inject(true)] Config Config { get; set; }
        [Inject(true)] Container Container { get; set; }
        [Inject(true)] Db Db { get; set; }

        TelegramBotClient Client { get; set; }
        ChatCommandRouter Commands;
        User Me { get; set; }

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

            string s = message.Text;
            long id = message.Chat.Id;
            long from = message.From.Id;
            Task.Run(() => Commands.Execute(sender, messageEventArgs));
        }

        void InitCommands(Container c)
        {
            Commands = new ChatCommandRouter(Me);
            Commands.Add(c.ResolveObject(new StartCommand()));
            Commands.Add(c.ResolveObject(new AwardCommand(Me)));
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
