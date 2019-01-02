using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Perfusion;
using JetKarmaBot.Services;

namespace JetKarmaBot.Commands
{
    class AwardCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "award", "revoke" };

        public bool Execute(CommandString cmd, MessageEventArgs args)
        {
            using (var db = Db.GetContext())
            {
                var currentLocale = Locale[db.Chat.Find(args.Message.Chat.Id).Locale];
                if (args.Message.ReplyToMessage == null)
                {
                    Client.SendTextMessageAsync(args.Message.Chat.Id, currentLocale["jetkarmabot.award.errawardnoreply"]);
                    return true;
                }

                var awarder = args.Message.From;
                var recipient = args.Message.ReplyToMessage.From;

                bool awarding = cmd.Command == "award";

                if (awarder.Id == recipient.Id)
                {
                    Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.award.errawardself"],
                        replyToMessageId: args.Message.MessageId);
                    return true;
                }

                if (Me.Id == recipient.Id)
                {
                    Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        awarding
                        ? currentLocale["jetkarmabot.award.errawardbot"]
                        : currentLocale["jetkarmabot.award.errrevokebot"],
                        replyToMessageId: args.Message.MessageId);
                    return true;
                }

                var text = args.Message.Text;
                var awardTypeText = cmd.Parameters.FirstOrDefault();
                var awardType = awardTypeText != null
                    ? db.Awardtype.First(at => at.Commandname == awardTypeText)
                    : db.Awardtype.Find(1);

                db.Award.Add(new Models.Award()
                {
                    Awardtypeid = awardType.Awardtypeid,
                    Amount = (sbyte)(awarding ? 1 : -1),
                    Fromid = awarder.Id,
                    Toid = recipient.Id,
                    Chatid = args.Message.Chat.Id
                });

                db.SaveChanges();

                string message = awarding
                    ? string.Format(currentLocale["jetkarmabot.award.awardmessage"], awardType.Name, "@" + recipient.Username)
                    : string.Format(currentLocale["jetkarmabot.award.revokemessage"], awardType.Name, "@" + recipient.Username);

                var currentCount = db.Award
                    .Where(aw => aw.Toid == recipient.Id && aw.Awardtypeid == awardType.Awardtypeid)
                    .Sum(aw => aw.Amount);

                var response = message + "\n" + String.Format(currentLocale["jetkarmabot.award.statustext"], "@" + recipient.Username, currentCount, awardType.Symbol);

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
        User Me { get; }

        public AwardCommand(User me)
        {
            Me = me;
        }
    }
}
