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
        public IReadOnlyCollection<string> Names => new[] { "award"};

        public bool Execute(object sender, MessageEventArgs args)
        {
            //var mentions = args.Message.Entities.Where(e => e.Type == MessageEntityType.Mention).ToArray();
            if (args.Message.ReplyToMessage == null)// && !mentions.Any())
            {
                Client.SendTextMessageAsync(args.Message.Chat.Id, "Please use this command in reply to another user, or use a mention.");
                return true;
            }

            var awarder = args.Message.From;
            //var members = Client.get(,).Result;
            //var recipient = mentions.FirstOrDefault()?.User ?? args.Message.ReplyToMessage.From;
            var recipient = args.Message.ReplyToMessage.From;

            if (awarder.Id == recipient.Id)
            {
                Client.SendTextMessageAsync(args.Message.Chat.Id, "Please stop playing with yourself.");
                return true;
            }

            if (Me.Id == recipient.Id)
            {
                Client.SendTextMessageAsync(args.Message.Chat.Id, "I am a bot, and have no use for your foolish fake internet points.");
                return true;
            }

            var text = args.Message.Text;
            var command = CommandString.Parse(text);
            var awardTypeId = Db.GetAwardTypeId(command.Parameters.FirstOrDefault());
            var awardType = Db.AwardTypes[awardTypeId];
            Db.AddAward(awardTypeId, awarder.Id, recipient.Id, args.Message.Chat.Id);
            var response = $"Awarded a {awardType.Name} to {recipient.Username}!\n" +
                $"{recipient.Username} is at {Db.CountAwards(recipient.Id, awardTypeId)}{awardType.Symbol} now.";
            Client.SendTextMessageAsync(args.Message.Chat.Id, response);
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
