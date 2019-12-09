using System.Linq;
using System.Collections.Generic;
using Perfusion;
using JetKarmaBot.Services.Handling;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JetKarmaBot.Models;

namespace JetKarmaBot.Commands
{
    class StatusCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "status" };

        public async Task<bool> Execute(RequestContext ctx)
        {
            var db = ctx.GetFeature<KarmaContext>();
            var currentLocale = ctx.GetFeature<Locale>();
            var asker = ctx.EventArgs.Message.From;
            var awardTypeName = ctx.Command.Parameters.FirstOrDefault();
            bool isPrivate = ctx.EventArgs.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private;

            string response;

            if (string.IsNullOrWhiteSpace(awardTypeName))
            {
                // var awards = db.Awards.Where(x => x.ToId == asker.Id)
                // .GroupBy(x => x.AwardTypeId)
                // .Select(x => new { AwardTypeId = x.Key, Amount = x.Sum(y => y.Amount) });
                if (!db.Awards.Any(x => x.ToId == asker.Id && (x.ChatId == ctx.EventArgs.Message.Chat.Id || isPrivate)))
                    response = currentLocale["jetkarmabot.status.havenothing"];
                else
                {
                    var awardsQuery = from award in db.Awards
                                      where award.ToId == asker.Id && (award.ChatId == ctx.EventArgs.Message.Chat.Id || isPrivate)
                                      group award by award.AwardTypeId into g
                                      join awardType in db.AwardTypes
                                      on g.Key equals awardType.AwardTypeId
                                      select new { AwardTypeId = g.Key, AwardTypeSymbol = awardType.Symbol, Amount = g.Sum(x => x.Amount) };
                    var awardsByType = await awardsQuery.ToListAsync();
                    response = currentLocale["jetkarmabot.status.listalltext"] + "\n"
                         + string.Join("\n",
                             awardsByType.Select(a => $" - {a.AwardTypeSymbol} {a.Amount}")
                        );

                }
            }
            else
            {
                var awardTypeIdQuery = from awt in db.AwardTypes
                                       where awt.CommandName == awardTypeName
                                       select awt.AwardTypeId;
                var awardTypeId = await awardTypeIdQuery.FirstAsync();
                var awardType = await db.AwardTypes.FindAsync(awardTypeId);

                response = string.Format(currentLocale["jetkarmabot.status.listspecifictext"],
                    await db.Awards.Where(
                         x => x.AwardTypeId == awardTypeId
                      && x.ToId == asker.Id
                      && x.ChatId == ctx.EventArgs.Message.Chat.Id)
                    .SumAsync(x => x.Amount), awardType.Symbol);
            }

            await ctx.SendMessage(response);
            return true;
        }

        [Inject] Localization Locale { get; set; }

        public string Description => "Shows the amount of awards that you have";
        public string DescriptionID => "jetkarmabot.status.help";

        public IReadOnlyCollection<ChatCommandArgument> Arguments => new ChatCommandArgument[] {
            new ChatCommandArgument(){
                Name="awardtype",
                Required=false,
                Type=ChatCommandArgumentType.String,
                Description="The awardtype to show. If empty shows everything.",
                DescriptionID= "jetkarmabot.status.awardtypehelp"
            }
        };
    }
}
