using Microsoft.EntityFrameworkCore;
using NLog;
using JetKarmaBot.Services.Handling;
using JetKarmaBot.Models;

namespace JetKarmaBot.Commands;

class AwardCommand : IChatCommand
{
    public IReadOnlyCollection<string> Names => new[] { "award", "revoke" };
    [Inject] private Logger log;

    public async Task<bool> Execute(RequestContext ctx)
    {
        var db = ctx.GetFeature<KarmaContext>();
        var currentLocale = ctx.GetFeature<Locale>();

        var awarder = ctx.EventArgs.Message.From;

        if (Timeout.TimeoutCache[awarder.Id].PreviousAwardDate.AddSeconds(Config.Timeout.AwardTimeSeconds) > DateTime.Now)
        {
            ctx.GetFeature<TimeoutManager.Feature>().Multiplier = 0; // Doesn't count as success or failure
            if (!Timeout.TimeoutCache[awarder.Id].TimeoutMessaged)
                await ctx.SendMessage(currentLocale["jetkarmabot.ratelimit"]);
            Timeout.TimeoutCache[awarder.Id].TimeoutMessaged = true;
            return false;
        }

        string awardTypeText = null;
        long recipientId = default;
        foreach (string arg in ctx.Command.Parameters)
        {
            if (arg.StartsWith('@'))
            {
                if (recipientId != default(int))
                {
                    await ctx.SendMessage(currentLocale["jetkarmabot.award.errdup"]);
                    return false;
                }
                recipientId = await db.Users.Where(x => x.Username == arg).Select(x => x.UserId).FirstOrDefaultAsync();
                if (recipientId == default(int))
                {
                    await ctx.SendMessage(currentLocale["jetkarmabot.award.errbadusername"]);
                    return false;
                }
            }
            else
            {
                if (awardTypeText == null)
                    awardTypeText = arg;
                else
                {
                    await ctx.SendMessage(currentLocale["jetkarmabot.award.errdup"]);
                    return false;
                }
            }
        }

        if (ctx.EventArgs.Message.ReplyToMessage != null && recipientId == default)
        {
            recipientId = ctx.EventArgs.Message.ReplyToMessage.From.Id;
        }

        if (recipientId == default(int))
        {
            await ctx.SendMessage(currentLocale["jetkarmabot.award.errawardnoreply"]);
            return false;
        }


        bool awarding = ctx.Command.Command == "award";

        if (awarder.Id == recipientId)
        {
            await ctx.SendMessage(currentLocale["jetkarmabot.award.errawardself"]);
            return false;
        }

        if (ctx.GetFeature<ChatCommandRouter.Feature>().Router.Me.Id == recipientId)
        {
            await ctx.SendMessage(awarding
                ? currentLocale["jetkarmabot.award.errawardbot"]
                : currentLocale["jetkarmabot.award.errrevokebot"]);
            return false;
        }

        var text = ctx.EventArgs.Message.Text;
        global::JetKarmaBot.Models.AwardType awardType = awardTypeText != null
            ? await db.AwardTypes.FirstAsync(at => at.CommandName == awardTypeText)
            : await db.AwardTypes.FindAsync((sbyte)1);

        var prevCount = await db.Awards
            .Where(aw => aw.ToId == recipientId && aw.AwardTypeId == awardType.AwardTypeId && aw.ChatId == ctx.EventArgs.Message.Chat.Id)
            .SumAsync(aw => aw.Amount);

        await db.Awards.AddAsync(new Models.Award()
        {
            AwardTypeId = awardType.AwardTypeId,
            Amount = (sbyte)(awarding ? 1 : -1),
            FromId = awarder.Id,
            ToId = recipientId,
            ChatId = ctx.EventArgs.Message.Chat.Id
        });

        var recUserName = (await db.Users.FindAsync(recipientId)).Username;

        log.Debug($"Awarded {(awarding ? 1 : -1)}{awardType.Symbol} to {recUserName}");

        string message = awarding
            ? string.Format(currentLocale["jetkarmabot.award.awardmessage"], getLocalizedName(awardType, currentLocale), recUserName)
            : string.Format(currentLocale["jetkarmabot.award.revokemessage"], getLocalizedName(awardType, currentLocale), recUserName);


        var response = message + "\n" + String.Format(currentLocale["jetkarmabot.award.statustext"], recUserName, prevCount + (awarding ? 1 : -1), awardType.Symbol);

        await ctx.SendMessage(response);
        Timeout.TimeoutCache[awarder.Id].PreviousAwardDate = DateTime.Now;
        return true;
    }

    private string getLocalizedName(global::JetKarmaBot.Models.AwardType awardType, Locale loc)
    {
        if (loc.ContainsKey($"jetkarmabot.awardtypes.accusative.{awardType.CommandName}"))
        {
            return loc[$"jetkarmabot.awardtypes.accusative.{awardType.CommandName}"];
        }
        else
        {
            return awardType.Name;
        }
    }

    [Inject] Localization Locale { get; set; }
    [Inject] TimeoutManager Timeout { get; set; }
    [Inject] Config Config { get; set; }

    public string Description => "Awards/revokes an award to a user.";
    public string DescriptionID => "jetkarmabot.award.help";

    public IReadOnlyCollection<ChatCommandArgument> Arguments => new ChatCommandArgument[] {
        new ChatCommandArgument() {
            Name="awardtype",
            Required=false,
            Type=ChatCommandArgumentType.String,
            Description="The award to grant to/strip of the specified user",
            DescriptionID="jetkarmabot.award.awardtypehelp"
        },
        new ChatCommandArgument() {
            Name="to",
            Required=false,
            Type=ChatCommandArgumentType.String,
            Description="The user to award it to.",
            DescriptionID="jetkarmabot.award.tohelp"
        }
    };
}
