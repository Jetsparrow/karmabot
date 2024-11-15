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
        public IReadOnlyCollection<string> Names => ["status"];

        public async Task<bool> Execute(RequestContext ctx)
        {
            var db = ctx.GetFeature<KarmaContext>();
            var currentLocale = ctx.GetFeature<Locale>();
            var asker = ctx.EventArgs.Message.From;
            bool isPrivate = ctx.EventArgs.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private;

            string response;

            var awards = db.Awards.Where(x => x.ToId == asker.Id);
            if (!isPrivate)
                awards = awards.Where(x => x.ChatId == ctx.EventArgs.Message.Chat.Id);

            if (!awards.Any())
                response = currentLocale["jetkarmabot.status.havenothing"];
            else
            {
                var aq = db.AwardTypes.GroupJoin(
                    awards, 
                    at => at.AwardTypeId, 
                    aw => aw.AwardTypeId, 
                    (at, aws)  => new {at.Symbol, Amount = aws.Sum(aw => aw.Amount) });
             
                var awardsByType = await aq.ToListAsync();
                
                response = 
                    currentLocale["jetkarmabot.status.listalltext"] + "\n"
                    + string.Join("\n", awardsByType.Select(a => $" - {a.Symbol} {a.Amount}"));
            }

            await ctx.SendMessage(response);
            return true;
        }

        [Inject] Localization Locale { get; set; }

        public string Description => "Shows the amount of awards that you have";
        public string DescriptionID => "jetkarmabot.status.help";

        public IReadOnlyCollection<ChatCommandArgument> Arguments => [];
    }
}
