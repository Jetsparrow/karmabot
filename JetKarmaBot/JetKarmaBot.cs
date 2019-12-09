using JetKarmaBot.Commands;
using JetKarmaBot.Models;
using JetKarmaBot.Services;
using JetKarmaBot.Services.Handling;
using Perfusion;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Args;
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
        RequestChain Chain;
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
            InitChain(Container);

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

            RequestContext ctx = new RequestContext(Client, args, cmd);
            _ = Chain.Handle(ctx);
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

        void InitChain(IContainer c)
        {
            Chain = c.ResolveObject(new RequestChain());
            Chain.Add(c.GetInstance<TimeoutManager.PreDbThrowout>());
            Chain.Add(c.GetInstance<DatabaseHandler>());
            Chain.Add(Timeout);
            Chain.Add(c.GetInstance<SaveData>());
            Chain.Add(Commands);
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
