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
                Client.SendTextMessageAsync(args.Message.Chat.Id, Locale["jetkarmabot.award.errawardnoreply", "en-US"]);
                return true;
            }

            var awarder = args.Message.From;
            var recipient = args.Message.ReplyToMessage.From;

            bool awarding = cmd.Command == "award";

            if (awarder.Id == recipient.Id)
            {
                Client.SendTextMessageAsync(
                    args.Message.Chat.Id,
                    Locale["jetkarmabot.award.errawardself", "en-US"],
                    replyToMessageId: args.Message.MessageId);
                return true;
            }

            if (Me.Id == recipient.Id)
            {
                Client.SendTextMessageAsync(
                    args.Message.Chat.Id,
                    awarding
                    ? Locale["jetkarmabot.award.errawardbot", "en-US"]
                    : Locale["jetkarmabot.award.errrevokebot", "en-US"],
                    replyToMessageId: args.Message.MessageId);
                return true;
            }

            var text = args.Message.Text;
            var awardTypeId = Db.GetAwardTypeId(cmd.Parameters.FirstOrDefault());
            var awardType = Db.AwardTypes[awardTypeId];

            Db.AddAward(awardTypeId, awarder.Id, recipient.Id, args.Message.Chat.Id, awarding ? 1 : -1);

            string message = awarding
                ? string.Format(Locale["jetkarmabot.award.awardmessage", "en-US"], awardType.Name, "@" + recipient.Username)
                : string.Format(Locale["jetkarmabot.award.revokemessage", "en-US"], awardType.Name, "@" + recipient.Username);

            var response = message + "\n" + String.Format(Locale["jetkarmabot.award.statustext", "en-US"], "@" + recipient.Username, Db.CountUserAwards(recipient.Id, awardTypeId), awardType.Symbol);

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
