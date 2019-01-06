using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Perfusion;
using JetKarmaBot.Services;
using NLog;

namespace JetKarmaBot.Commands
{
    class LocaleCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "changelocale", "locale" };
        private static Logger log = LogManager.GetCurrentClassLogger();

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
                string localeId;
                if (Locale.ContainsLocale(cmd.Parameters[0]))
                    localeId = cmd.Parameters[0];
                else
                    localeId = Locale.FindByCommonName(cmd.Parameters[0]).Name;
                db.Chats.Find(args.Message.Chat.Id).Locale = localeId;
                log.Debug($"Changed language of chat {args.Message.Chat.Id} to {localeId}");
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
