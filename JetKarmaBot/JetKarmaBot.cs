using JetKarmaBot.Commands;
using JetKarmaBot.Models;
using JetKarmaBot.Services;
using JetKarmaBot.Services.Handling;
using NLog;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetKarmaBot;

public class JetKarmaBot : IDisposable
{
    [Inject] Config Config { get; set; }
    [Inject] IContainer Container { get; set; }
    [Inject] KarmaContextFactory Db { get; set; }
    [Inject] TimeoutManager Timeout { get; set; }
    [Inject] Localization Locale { get; set; }
    [Inject] Logger Log { get; set; }


    TelegramBotClient Client { get; set; }
    ChatCommandRouter Commands;
    RequestChain Chain;
    Task timeoutWaitTask;
    CancellationTokenSource timeoutWaitTaskToken;
    private bool stopped = false;

    public async Task Init()
    {
        using (KarmaContext db = Db.GetContext())
            await db.Database.EnsureCreatedAsync();
        
        Client = new TelegramBotClient(Config.ApiKey);
        Container.AddInstance(Client);

        timeoutWaitTaskToken = new CancellationTokenSource();
        timeoutWaitTask = Timeout.SaveLoop(timeoutWaitTaskToken.Token);

        await InitCommands(Container);
        InitChain(Container);

        var receiverOptions = new ReceiverOptions { AllowedUpdates = new[] { UpdateType.Message } };
        Client.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions);
    }

    public async Task Stop()
    {
        if (stopped) return;
        Client?.CloseAsync();
        timeoutWaitTaskToken?.Cancel();
        try
        {
            if (timeoutWaitTask != null)
                await timeoutWaitTask;
        }
        catch (OperationCanceledException) { }
        await Timeout?.Save();
        Dispose();
        stopped = true;
    }

    #region service

    Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Log.Error(exception, "Exception while handling API message");
        return Task.CompletedTask;
    }

    async Task HandleUpdateAsync(ITelegramBotClient sender, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message || update?.Message?.Type != MessageType.Text)
            return;
        var message = update.Message!;

        try
        {
            if (message == null || message.Type != MessageType.Text)
                return;
            if (!CommandString.TryParse(message.Text, out var cmd))
                return;
            if (cmd.UserName != null && cmd.UserName != Commands.Me.Username)
                return;

            RequestContext ctx = new RequestContext(Client, update, cmd);
            await Chain.Handle(ctx);
        }
        catch (Exception e)
        {
            Log.Error(e, "Exception while handling message {0}", message);
        }
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
        timeoutWaitTaskToken?.Dispose();
        timeoutWaitTask?.Dispose();
    }

    #endregion
}
