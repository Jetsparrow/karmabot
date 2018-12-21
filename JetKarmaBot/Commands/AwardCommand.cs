using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Perfusion;

namespace JetKarmaBot.Commands
{
    class AwardCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "award", "revoke" };

        public bool Execute(CommandString cmd, MessageEventArgs args)
        {
            if (args.Message.ReplyToMessage == null)
            {
                Client.SendTextMessageAsync(args.Message.Chat.Id, Locale["jetkarmabot.award.errawardnoreply"]);
                return true;
            }

            var awarder = args.Message.From;
            var recipient = args.Message.ReplyToMessage.From;

            bool awarding = cmd.Command == "award";

            if (awarder.Id == recipient.Id)
            {
                Client.SendTextMessageAsync(
                    args.Message.Chat.Id,
                    Locale["jetkarmabot.award.errawardself"],
                    replyToMessageId: args.Message.MessageId);
                return true;
            }

            if (Me.Id == recipient.Id)
            {
                Client.SendTextMessageAsync(
                    args.Message.Chat.Id,
                    awarding
                    ? Locale["jetkarmabot.award.errawardbot"]
                    : Locale["jetkarmabot.award.errrevokebot"],
                    replyToMessageId: args.Message.MessageId);
                return true;
            }

            var text = args.Message.Text;
            var awardTypeId = Db.GetAwardTypeId(cmd.Parameters.FirstOrDefault());
            var awardType = Db.AwardTypes[awardTypeId];

            Db.AddAward(awardTypeId, awarder.Id, recipient.Id, args.Message.Chat.Id, awarding ? 1 : -1);

            string message = awarding
                ? string.Format(Locale["jetkarmabot.award.awardmessage"], awardType.Name, "@" + recipient.Username)
                : string.Format(Locale["jetkarmabot.award.revokemessage"], awardType.Name, "@" + recipient.Username);

            var response = message + "\n" + String.Format(Locale["jetkarmabot.award.statustext"], "@" + recipient.Username, Db.CountUserAwards(recipient.Id, awardTypeId), awardType.Symbol);

            Client.SendTextMessageAsync(
                args.Message.Chat.Id,
                response,
                replyToMessageId: args.Message.MessageId);
            return true;
        }

        [Inject(true)] Db Db { get; set; }
        [Inject(true)] TelegramBotClient Client { get; set; }
        [Inject(true)] Localization Locale { get; set; }
        User Me { get; }

        public AwardCommand(User me)
        {
            Me = me;
        }
    }
}
