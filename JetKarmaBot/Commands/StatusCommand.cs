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
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JetKarmaBot.Commands
{
    class StatusCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "status" };

        public async Task<bool> Execute(CommandString cmd, MessageEventArgs args)
        {
            using (var db = Db.GetContext())
            {
                var currentLocale = Locale[(await db.Chats.FindAsync(args.Message.Chat.Id)).Locale];
                var asker = args.Message.From;
                var awardTypeName = cmd.Parameters.FirstOrDefault();
                bool isPrivate = args.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private;

                string response;

                if (string.IsNullOrWhiteSpace(awardTypeName))
                {
                    // var awards = db.Awards.Where(x => x.ToId == asker.Id)
                    // .GroupBy(x => x.AwardTypeId)
                    // .Select(x => new { AwardTypeId = x.Key, Amount = x.Sum(y => y.Amount) });
                    if (!db.Awards.Any(x => x.ToId == asker.Id && (x.ChatId == args.Message.Chat.Id || isPrivate)))
                        response = currentLocale["jetkarmabot.status.havenothing"];
                    else
                    {
                        var awardsQuery = from award in db.Awards
                                          where award.ToId == asker.Id && (award.ChatId == args.Message.Chat.Id || isPrivate)
                                          group award by award.AwardTypeId into g
                                          select new { AwardTypeId = g.Key, Amount = g.Sum(x => x.Amount) };
                        var awardsByType = await awardsQuery.ToListAsync();
                        response = currentLocale["jetkarmabot.status.listalltext"] + "\n"
                             + string.Join("\n", await Task.WhenAll(
                                 awardsByType.Select(async a => $" - {(await db.AwardTypes.FindAsync(a.AwardTypeId)).Symbol} {a.Amount}")
                            ));

                    }
                }
                else
                {
                    var awardTypeIdQuery = from awt in db.AwardTypes
                                           where awt.CommandName == awardTypeName
                                           select awt.AwardTypeId;
                    var awardTypeId = await awardTypeIdQuery.FirstAsync();
                    var awardType = await db.AwardTypes.FindAsync(awardTypeId);

                    response = string.Format(currentLocale["jetkarmabot.status.listspecifictext"], await db.Awards.Where(x => x.AwardTypeId == awardTypeId && x.ToId == asker.Id && x.ChatId == args.Message.Chat.Id).SumAsync(x => x.Amount), awardType.Symbol);
                }

                await Client.SendTextMessageAsync(
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
