using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Perfusion;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace JetKarmaBot.Commands
{
    class StatusCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "status" };

        public bool Execute(CommandString cmd, MessageEventArgs args)
        {
            var asker = args.Message.From;
            var awardTypeName = cmd.Parameters.FirstOrDefault();

            string response;

            if (string.IsNullOrWhiteSpace(awardTypeName))
            {
                var awards = Db.CountAllUserAwards(asker.Id);

                response = Locale["jetkarmabot.status.listalltext", "en-US"] + "\n"
                     + string.Join("\n", awards.Select(a => $" - {Db.AwardTypes[a.AwardTypeId].Symbol} {a.Amount}"));

            }
            else
            {
                var awardTypeId = Db.GetAwardTypeId(cmd.Parameters.FirstOrDefault());
                var awardType = Db.AwardTypes[awardTypeId];

                response = string.Format(Locale["jetkarmabot.status.listspecifictext", "en-US"], Db.CountUserAwards(asker.Id, awardTypeId), awardType.Symbol);
            }

            Client.SendTextMessageAsync(
                args.Message.Chat.Id,
                response,
                replyToMessageId: args.Message.MessageId);
            return true;
        }

        [Inject(true)] Db Db { get; set; }
        [Inject(true)] TelegramBotClient Client { get; set; }
        [Inject(true)] Localization Locale { get; set; }

    }
}
