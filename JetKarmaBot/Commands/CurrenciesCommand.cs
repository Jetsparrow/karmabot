using System.Collections.Generic;
using Telegram.Bot.Args;
using Perfusion;
using JetKarmaBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JetKarmaBot.Commands
{
    public class CurrenciesCommand : IChatCommand
    {
        [Inject] KarmaContextFactory Db;
        [Inject] TelegramBotClient Client { get; set; }
        [Inject] Localization Locale { get; set; }
        public IReadOnlyCollection<string> Names => new[] { "currencies", "awardtypes" };

        public string Description => "Shows all award types";
        public string DescriptionID => "jetkarmabot.currencies.help";

        public IReadOnlyCollection<ChatCommandArgument> Arguments => new ChatCommandArgument[] {
         };

        public async Task<bool> Execute(CommandString cmd, MessageEventArgs args)
        {
            using (var db = Db.GetContext())
            {
                var currentLocale = Locale[(await db.Chats.FindAsync(args.Message.Chat.Id)).Locale];
                string resp = currentLocale["jetkarmabot.currencies.listtext"] + "\n" + string.Join("\n",
                (await db.AwardTypes.ToListAsync()).Select(x => $"{x.Symbol} ({x.CommandName}) <i>{currentLocale["jetkarmabot.awardtypes.nominative." + x.CommandName]}</i>"));
                await Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        resp,
                        replyToMessageId: args.Message.MessageId,
                        parseMode: ParseMode.Html);
                return true;
            }
        }
    }
}