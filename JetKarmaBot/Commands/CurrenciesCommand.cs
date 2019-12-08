using System.Collections.Generic;
using Perfusion;
using JetKarmaBot.Services.Handling;
using Telegram.Bot.Types.Enums;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JetKarmaBot.Commands
{
    public class CurrenciesCommand : IChatCommand
    {
        [Inject] Localization Locale { get; set; }
        public IReadOnlyCollection<string> Names => new[] { "currencies", "awardtypes" };

        public string Description => "Shows all award types";
        public string DescriptionID => "jetkarmabot.currencies.help";

        public IReadOnlyCollection<ChatCommandArgument> Arguments => new ChatCommandArgument[] {
         };

        public async Task<bool> Execute(RequestContext ctx)
        {
            var db = ctx.Database;
            var currentLocale = Locale[(await db.Chats.FindAsync(ctx.EventArgs.Message.Chat.Id)).Locale];
            await ctx.SendMessage(
                currentLocale["jetkarmabot.currencies.listtext"] + "\n" + string.Join("\n",
                    (await db.AwardTypes.ToListAsync())
                        .Select(x => $"{x.Symbol} ({x.CommandName}) <i>{currentLocale["jetkarmabot.awardtypes.nominative." + x.CommandName]}</i>")));
            return true;
        }
    }
}