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
        [Inject] IContainer Container { get; set; }
        [Inject] KarmaContextFactory Db { get; set; }

        TelegramBotClient Client { get; set; }
        ChatCommandRouter Commands;

        public async Task Init()
        {
            using (KarmaContext db = Db.GetContext())
                await db.Database.EnsureCreatedAsync();
            var httpProxy = new WebProxy($"{Config.Proxy.Url}:{Config.Proxy.Port}")
            {
                Credentials = new NetworkCredential(Config.Proxy.Login, Config.Proxy.Password)
            };

            Client = new TelegramBotClient(Config.ApiKey, httpProxy);
            Container.AddInstance(Client);

            await InitCommands(Container);

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

            Task.Run(async () =>
            {
                using (KarmaContext db = Db.GetContext())
                {
                    await AddUserToDatabase(db, messageEventArgs.Message.From);
                    if (messageEventArgs.Message.ReplyToMessage != null)
                        await AddUserToDatabase(db, messageEventArgs.Message.ReplyToMessage.From);
                    if (!db.Chats.Any(x => x.ChatId == messageEventArgs.Message.Chat.Id))
                        db.Chats.Add(new Models.Chat { ChatId = messageEventArgs.Message.Chat.Id });
                    await db.SaveChangesAsync();
                }
                await Commands.Execute(null, messageEventArgs);
            });
        }

        private async Task AddUserToDatabase(KarmaContext db, Telegram.Bot.Types.User u)
        {
            string un;
            if (u.Username == null)
                un = u.FirstName + (u.LastName != null ? " " + u.LastName : "");
            else
                un = "@" + u.Username;
            if (!db.Users.Any(x => x.UserId == u.Id))
                await db.Users.AddAsync(new Models.User { UserId = u.Id, Username = un });
            else
                (await db.Users.FindAsync(u.Id)).Username = un;
        }

        async Task InitCommands(IContainer c)
        {
            Commands = c.ResolveObject(new ChatCommandRouter());
            Commands.Me = await Client.GetMeAsync();
            Commands.Add(c.GetInstance<HelpCommand>());
            Commands.Add(c.GetInstance<AwardCommand>());
            Commands.Add(c.GetInstance<StatusCommand>());
            Commands.Add(c.GetInstance<LocaleCommand>());
            Commands.Add(c.GetInstance<CurrenciesCommand>());
            Commands.Add(c.GetInstance<LeaderboardCommand>());
            Commands.Add(c.GetInstance<AwardTypeCommand>());
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
