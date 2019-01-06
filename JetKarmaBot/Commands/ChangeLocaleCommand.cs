using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Perfusion;
using JetKarmaBot.Services;

namespace JetKarmaBot.Commands
{
    class LocaleCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "changelocale", "locale" };

        public bool Execute(CommandString cmd, MessageEventArgs args)
        {
            using (var db = Db.GetContext())
            {
                var currentLocale = Locale[db.Chats.Find(args.Message.Chat.Id).Locale];
                if (cmd.Parameters.Length < 1)
                {
                    Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.changelocale.getlocale"],
                        replyToMessageId: args.Message.MessageId);
                    return true;
                }
                db.Chats.Find(args.Message.Chat.Id).Locale = cmd.Parameters[0];
                db.SaveChanges();

                currentLocale = Locale[db.Chats.Find(args.Message.Chat.Id).Locale];

                Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.changelocale.justchanged"],
                        replyToMessageId: args.Message.MessageId);
                return true;
            }
        }

        [Inject] KarmaContextFactory Db { get; set; }
        [Inject] TelegramBotClient Client { get; set; }
        [Inject] Localization Locale { get; set; }
    }
}
