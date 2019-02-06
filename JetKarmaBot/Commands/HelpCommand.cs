using System.Collections.Generic;
using Telegram.Bot.Args;
using Perfusion;
using JetKarmaBot.Services;
using Telegram.Bot;

namespace JetKarmaBot.Commands
{
    public class HelpCommand : IChatCommand
    {
        [Inject] KarmaContextFactory Db;
        [Inject] TelegramBotClient Client { get; set; }
        [Inject] Localization Locale { get; set; }
        public IReadOnlyCollection<string> Names => new[] { "help" };

        public bool Execute(CommandString cmd, MessageEventArgs args)
        {
            using (var db = Db.GetContext())
            {
                var currentLocale = Locale[db.Chats.Find(args.Message.Chat.Id).Locale];
                Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.help"],
                        replyToMessageId: args.Message.MessageId);
                return true;
            }
        }
    }
}
