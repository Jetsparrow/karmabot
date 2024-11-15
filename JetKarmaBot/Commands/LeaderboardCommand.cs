using Microsoft.EntityFrameworkCore;
using JetKarmaBot.Services.Handling;
using JetKarmaBot.Models;

namespace JetKarmaBot.Commands;

class LeaderboardCommand : IChatCommand
{
    public IReadOnlyCollection<string> Names => new[] { "leaderboard" };

    public async Task<bool> Execute(RequestContext ctx)
    {
        bool isPrivate = ctx.EventArgs.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private;
        var currentLocale = ctx.GetFeature<Locale>();
        if (isPrivate)
        {
            await ctx.SendMessage(currentLocale["jetkarmabot.award.errawardself"]);
            return true;
        }

        var db = ctx.GetFeature<KarmaContext>();
        var asker = ctx.EventArgs.Message.From;
        var awardTypeName = ctx.Command.Parameters.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(awardTypeName))
            awardTypeName = "star";

        var awardTypeIdQuery = from awt in db.AwardTypes
                               where awt.CommandName == awardTypeName
                               select awt.AwardTypeId;
        var awardTypeId = await awardTypeIdQuery.FirstAsync();
        var awardType = await db.AwardTypes.FindAsync(awardTypeId);

        var topEarners = await db.Awards
                .Where(x => x.ChatId == ctx.EventArgs.Message.Chat.Id && x.AwardTypeId == awardTypeId)
                .GroupBy(x => x.To)
                .Select(x => new { User = x.Key,  Amount = x.Sum(y => y.Amount) })
                .OrderByDescending(x => x.Amount)
                .Take(5)
                .ToListAsync();

        var response = string.Format(currentLocale["jetkarmabot.leaderboard.specifictext"], awardType.Symbol) + "\n" 
            + string.Join('\n', topEarners.Select((x, index) 
            => $"{index + 1}. {x.User.Username} - {x.Amount}")
        );

        await ctx.SendMessage(response);
        return true;
    }

    [Inject] Localization Locale { get; set; }

    public string Description => "Shows the people with the most of a specific award.";
    public string DescriptionID => "jetkarmabot.leaderboard.help";

    public IReadOnlyCollection<ChatCommandArgument> Arguments => new ChatCommandArgument[] {
        new ChatCommandArgument() {
            Name="awardtype",
            Required=true,
            Type=ChatCommandArgumentType.String,
            Description="The awardtype to show a leaderboard for.",
            DescriptionID= "jetkarmabot.leaderboard.awardtypehelp"
        }
    };
}
