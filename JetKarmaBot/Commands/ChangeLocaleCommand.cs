using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Perfusion;
using JetKarmaBot.Models;
using JetKarmaBot.Services;

namespace JetKarmaBot.Commands
{
    class ChangeLocaleCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "changelocale", "locale" };

        public bool Execute(CommandString cmd, MessageEventArgs args)
        {
            var currentLocale = Locale[Db.Chats[args.Message.Chat.Id].Locale];
            if (cmd.Parameters.Length < 1)
            {
                Client.SendTextMessageAsync(
                    args.Message.Chat.Id,
                    currentLocale["jetkarmabot.changelocale.getlocale"],
                    replyToMessageId: args.Message.MessageId);
                return true;
            }
            Db.ChangeChatLocale(Db.Chats[args.Message.Chat.Id], cmd.Parameters[0]);
            currentLocale = Locale[Db.Chats[args.Message.Chat.Id].Locale];
            Client.SendTextMessageAsync(
                    args.Message.Chat.Id,
                    currentLocale["jetkarmabot.changelocale.justchanged"],
                    replyToMessageId: args.Message.MessageId);
            return true;
        }

        [Inject] KarmaContextFactory Db { get; set; }
        [Inject] TelegramBotClient Client { get; set; }
        [Inject] Localization Locale { get; set; }
    }
}