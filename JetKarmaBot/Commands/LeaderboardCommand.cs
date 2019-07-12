using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Perfusion;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using JetKarmaBot.Models;
using JetKarmaBot.Services;

namespace JetKarmaBot.Commands
{
    class LeaderboardCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "leaderboard" };

        public bool Execute(CommandString cmd, MessageEventArgs args)
        {
            using (var db = Db.GetContext())
            {
                var currentLocale = Locale[db.Chats.Find(args.Message.Chat.Id).Locale];
                var asker = args.Message.From;
                var awardTypeName = cmd.Parameters.FirstOrDefault();

                string response;

                if (string.IsNullOrWhiteSpace(awardTypeName))
                    awardTypeName = "star";

                var awardTypeIdQuery = from awt in db.AwardTypes
                                       where awt.CommandName == awardTypeName
                                       select awt.AwardTypeId;
                var awardTypeId = awardTypeIdQuery.First();
                var awardType = db.AwardTypes.Find(awardTypeId);

                response = string.Format(currentLocale["jetkarmabot.leaderboard.specifictext"], awardType.Symbol) + "\n" + string.Join('\n', db.Awards
                        .Where(x => x.ChatId == args.Message.Chat.Id && x.AwardTypeId == awardTypeId)
                        .GroupBy(x => x.ToId)
                        .Select(x => new {UserId = x.Key, Amount = x.Sum(y => y.Amount)})
                        .OrderByDescending(x => x.Amount)
                        .Take(5)
                        .ToList()
                        .Select((x,index) => $"{index+1}. {db.Users.Find(x.UserId).Username} - {x.Amount}"));

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
