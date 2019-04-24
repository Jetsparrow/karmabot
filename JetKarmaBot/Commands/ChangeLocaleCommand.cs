using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Perfusion;
using JetKarmaBot.Services;
using NLog;
using System.Linq;

namespace JetKarmaBot.Commands
{
    class LocaleCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "changelocale", "locale" };
        [Inject]
        private Logger log;

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
                else if (cmd.Parameters[0] == "list")
                {
                    Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.changelocale.listalltext"] + "\n"
                             + string.Join("\n", Locale.Select(a => a.Key)),
                        replyToMessageId: args.Message.MessageId);
                    return true;
                }
                else if (cmd.Parameters[0] == "all")
                {
                    Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.changelocale.errorall"],
                        replyToMessageId: args.Message.MessageId);
                    return true;
                }
                string localeId;
                if (Locale.ContainsLocale(cmd.Parameters[0]))
                    localeId = cmd.Parameters[0];
                else
                    try
                    {
                        localeId = Locale.FindByCommonName(cmd.Parameters[0]).Name;
                    }
                    catch (LocalizationException e)
                    {
                        Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.changelocale.toomany"] + "\n" + string.Join("\n", (e.Data["LocaleNames"] as Locale[]).Select(x => x.Name)),
                        replyToMessageId: args.Message.MessageId);
                        return true;
                    }
                db.Chats.Find(args.Message.Chat.Id).Locale = localeId;
                log.Debug($"Changed language of chat {args.Message.Chat.Id} to {localeId}");
                db.SaveChanges();

                currentLocale = Locale[db.Chats.Find(args.Message.Chat.Id).Locale];

                Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
(currentLocale.HasNote ? currentLocale["jetkarmabot.changelocale.beforenote"] + currentLocale.Note + "\n" : "")
                        + currentLocale["jetkarmabot.changelocale.justchanged"],
                        replyToMessageId: args.Message.MessageId);
                return true;
            }
        }

        [Inject] KarmaContextFactory Db { get; set; }
        [Inject] TelegramBotClient Client { get; set; }
        [Inject] Localization Locale { get; set; }

        public string Description => "Switches current chat locale to [locale]";
        public string DescriptionID => "jetkarmabot.changelocale.help";

        public IReadOnlyCollection<ChatCommandArgument> Arguments => new ChatCommandArgument[] {
            new ChatCommandArgument() {
                Name="locale",
                Required=false,
                Type=ChatCommandArgumentType.String,
                Description="The locale to switch to. Can be \"list\" to list all possible locales. Also can be empty to get current locale.",
                DescriptionID="jetkarmabot.changelocale.localehelp"
            }
        };
    }
}
