using JetKarmaBot.Commands;
using JetKarmaBot.Models;
using JetKarmaBot.Services;
using Perfusion;
using System;
using System.Linq;
using System.Net;
using System.Threading;
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
        [Inject] TimeoutManager Timeout { get; set; }
        [Inject] Localization Locale { get; set; }

        TelegramBotClient Client { get; set; }
        ChatCommandRouter Commands;
        Task timeoutWaitTask;
        CancellationTokenSource timeoutWaitTaskToken;

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

            timeoutWaitTaskToken = new CancellationTokenSource();
            timeoutWaitTask = Timeout.SaveLoop(timeoutWaitTaskToken.Token);

            await InitCommands(Container);

            Client.OnMessage += BotOnMessageReceived;
            Client.StartReceiving();
        }

        public async Task Stop()
        {
            Client.StopReceiving();
            timeoutWaitTaskToken.Cancel();
            try
            {
                await timeoutWaitTask;
            }
            catch (OperationCanceledException) { }
            await Timeout.Save();
            Dispose();
        }

        #region service

        void BotOnMessageReceived(object sender, MessageEventArgs args)
        {
            var message = args.Message;
            if (message == null || message.Type != MessageType.Text)
                return;
            if (!CommandString.TryParse(args.Message.Text, out var cmd))
                return;
            if (cmd.UserName != null && cmd.UserName != Commands.Me.Username)
                return;

            Task.Run(async () =>
            {
                using (KarmaContext db = Db.GetContext())
                {
                    await AddUserToDatabase(db, args.Message.From);
                    var checkResult = await Timeout.Check(args.Message.From.Id, db);
                    if (checkResult == TimeoutManager.CheckResult.Limited)
                    {
                        Locale currentLocale = Locale[(await db.Chats.FindAsync(args.Message.Chat.Id)).Locale];
                        await Client.SendTextMessageAsync(
                            args.Message.Chat.Id,
                            currentLocale["jetkarmabot.ratelimit"],
                            replyToMessageId: args.Message.MessageId);
                        await Timeout.SetMessaged(args.Message.From.Id, db);
                        return;
                    }
                    else if (checkResult != TimeoutManager.CheckResult.NonLimited)
                    {
                        return;
                    }
                    if (args.Message.ReplyToMessage != null)
                        await AddUserToDatabase(db, args.Message.ReplyToMessage.From);
                    if (!db.Chats.Any(x => x.ChatId == args.Message.Chat.Id))
                        db.Chats.Add(new Models.Chat
                        {
                            ChatId = args.Message.Chat.Id
                        });
                    await db.SaveChangesAsync();
                }
                await Commands.Execute(cmd, args);
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
            c.Add<HelpCommand>();
            c.Add<AwardCommand>();
            c.Add<StatusCommand>();
            c.Add<LocaleCommand>();
            c.Add<CurrenciesCommand>();
            c.Add<LeaderboardCommand>();
            Commands = c.GetInstance<ChatCommandRouter>();
            await Commands.Start();
            foreach (IChatCommand cmd in c.GetInstances<IChatCommand>())
            {
                Commands.Add(cmd);
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            timeoutWaitTaskToken.Dispose();
            timeoutWaitTask.Dispose();
        }

        #endregion
    }
}
