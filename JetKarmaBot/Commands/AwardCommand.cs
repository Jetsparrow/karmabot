using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetKarmaBot.Commands
{
    class AwardCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "award", "revoke"};

        public bool Execute(object sender, MessageEventArgs args)
        {
            if (args.Message.ReplyToMessage == null)
            {
                Client.SendTextMessageAsync(args.Message.Chat.Id, "Please use this command in reply to another user.");
                return true;
            }

            var awarder = args.Message.From;
            var recipient = args.Message.ReplyToMessage.From;

            if (awarder.Id == recipient.Id)
            {
                Client.SendTextMessageAsync(
                    args.Message.Chat.Id,
                    "Please stop playing with yourself.",
                    replyToMessageId: args.Message.MessageId);
                return true;
            }

            if (Me.Id == recipient.Id)
            {
                Client.SendTextMessageAsync(
                    args.Message.Chat.Id,
                    "I am a bot, and have no use for your foolish fake internet points.",
                    replyToMessageId: args.Message.MessageId);
                return true;
            }

            var text = args.Message.Text;
            var command = CommandString.Parse(text);
            var awardTypeId = Db.GetAwardTypeId(command.Parameters.FirstOrDefault());
            var awardType = Db.AwardTypes[awardTypeId];

            Db.AddAward(awardTypeId, awarder.Id, recipient.Id, args.Message.Chat.Id, 1);

            var response = $"Awarded a {awardType.Name} to {recipient.Username}!\n" +
                $"{recipient.Username} is at {Db.CountAwards(recipient.Id, awardTypeId)}{awardType.Symbol} now.";
            Client.SendTextMessageAsync(
                args.Message.Chat.Id,
                response,
                replyToMessageId: args.Message.MessageId);
            return true;
        }

        
        Db Db { get; }
        TelegramBotClient Client { get; }
        User Me { get; }

        public AwardCommand(Db db, TelegramBotClient client, User me)
        {
            Db = db;
            Client = client;
            Me = me;
        }
    }
}
