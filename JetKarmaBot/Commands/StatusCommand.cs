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

                response = "Your badges report:\n"
                     + string.Join("\n", awards.Select(a => $" - {Db.AwardTypes[a.AwardTypeId].Symbol} {a.Amount}"));

            }
            else
            {
                var awardTypeId = Db.GetAwardTypeId(cmd.Parameters.FirstOrDefault());
                var awardType = Db.AwardTypes[awardTypeId];

                response = $"You are at {Db.CountUserAwards(asker.Id, awardTypeId)}{awardType.Symbol} now.";
            }

            Client.SendTextMessageAsync(
                args.Message.Chat.Id,
                response,
                replyToMessageId: args.Message.MessageId);
            return true;
        }

        [Inject(true)] Db Db { get; set; }
        [Inject(true)] TelegramBotClient Client { get; set; }

    }
}
