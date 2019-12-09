using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Perfusion;
using JetKarmaBot.Services.Handling;
using JetKarmaBot.Models;

namespace JetKarmaBot.Commands
{
    class LeaderboardCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "leaderboard" };

        public async Task<bool> Execute(RequestContext ctx)
        {
            var db = ctx.GetFeature<KarmaContext>();
            var currentLocale = Locale[(await db.Chats.FindAsync(ctx.EventArgs.Message.Chat.Id)).Locale];
            var asker = ctx.EventArgs.Message.From;
            var awardTypeName = ctx.Command.Parameters.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(awardTypeName))
                awardTypeName = "star";

            var awardTypeIdQuery = from awt in db.AwardTypes
                                   where awt.CommandName == awardTypeName
                                   select awt.AwardTypeId;
            var awardTypeId = await awardTypeIdQuery.FirstAsync();
            var awardType = await db.AwardTypes.FindAsync(awardTypeId);

            await ctx.SendMessage(string.Format(currentLocale["jetkarmabot.leaderboard.specifictext"], awardType.Symbol) + "\n" + string.Join('\n',
                await Task.WhenAll((await db.Awards
                    .Where(x => x.ChatId == ctx.EventArgs.Message.Chat.Id && x.AwardTypeId == awardTypeId)
                    .GroupBy(x => x.ToId)
                    .Select(x => new { UserId = x.Key, Amount = x.Sum(y => y.Amount) })
                    .OrderByDescending(x => x.Amount)
                    .Take(5)
                    .ToListAsync())
                    .Select(async (x, index) => $"{index + 1}. {(await db.Users.FindAsync(x.UserId)).Username} - {x.Amount}"))
            ));
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
}
