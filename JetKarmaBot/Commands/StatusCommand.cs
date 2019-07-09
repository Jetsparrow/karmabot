using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Perfusion;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using JetKarmaBot.Models;
using JetKarmaBot.Services;

namespace JetKarmaBot.Commands
{
    class StatusCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "status" };

        public bool Execute(CommandString cmd, MessageEventArgs args)
        {
            using (var db = Db.GetContext())
            {
                var currentLocale = Locale[db.Chats.Find(args.Message.Chat.Id).Locale];
                var asker = args.Message.From;
                var awardTypeName = cmd.Parameters.FirstOrDefault();

                string response;

                if (string.IsNullOrWhiteSpace(awardTypeName))
                {
                    // var awards = db.Awards.Where(x => x.ToId == asker.Id)
                    // .GroupBy(x => x.AwardTypeId)
                    // .Select(x => new { AwardTypeId = x.Key, Amount = x.Sum(y => y.Amount) });
                    if (!db.Awards.Any(x => x.ToId == asker.Id))
                        response = currentLocale["jetkarmabot.status.havenothing"];
                    else
                    {
                        var awardsQuery = from award in db.Awards
                                          where award.ToId == asker.Id && award.ChatId == args.Message.Chat.Id
                                          group award by award.AwardTypeId into g
                                          select new { AwardTypeId = g.Key, Amount = g.Sum(x => x.Amount) };
                        var awardsByType = awardsQuery.ToList();
                        response = currentLocale["jetkarmabot.status.listalltext"] + "\n"
                             + string.Join("\n", awardsByType.Select(a => $" - {db.AwardTypes.Find(a.AwardTypeId).Symbol} {a.Amount}"));

                    }
                }
                else
                {
                    var awardTypeIdQuery = from awt in db.AwardTypes
                                           where awt.CommandName == awardTypeName
                                           select awt.AwardTypeId;
                    var awardTypeId = awardTypeIdQuery.First();
                    var awardType = db.AwardTypes.Find(awardTypeId);

                    response = string.Format(currentLocale["jetkarmabot.status.listspecifictext"], db.Awards.Where(x => x.AwardTypeId == awardTypeId && x.ToId == asker.Id && x.ChatId == args.Message.Chat.Id).Sum(x => x.Amount), awardType.Symbol);
                }

                Client.SendTextMessageAsync(
                    args.Message.Chat.Id,
                    response,
                    replyToMessageId: args.Message.MessageId);
                return true;
            }
        }

        [Inject] KarmaContextFactory Db { get; set; }
        [Inject] TelegramBotClient Client { get; set; }
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
