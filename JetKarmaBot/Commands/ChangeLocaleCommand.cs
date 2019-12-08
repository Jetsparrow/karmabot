using System.Collections.Generic;
using Perfusion;
using JetKarmaBot.Services.Handling;
using NLog;
using System.Linq;
using System.Threading.Tasks;

namespace JetKarmaBot.Commands
{
    class LocaleCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "changelocale", "locale" };
        [Inject] private Logger log;

        public async Task<bool> Execute(RequestContext ctx)
        {
            var db = ctx.Database;
            var cmd = ctx.Command;
            var args = ctx.EventArgs;

            var currentLocale = Locale[(await db.Chats.FindAsync(ctx.EventArgs.Message.Chat.Id)).Locale];
            if (cmd.Parameters.Length < 1)
            {
                await ctx.Client.SendTextMessageAsync(
                    args.Message.Chat.Id,
                    currentLocale["jetkarmabot.changelocale.getlocale"],
                    replyToMessageId: args.Message.MessageId);
                return false;
            }
            else if (cmd.Parameters[0] == "list")
            {
                await ctx.Client.SendTextMessageAsync(
                    args.Message.Chat.Id,
                    currentLocale["jetkarmabot.changelocale.listalltext"] + "\n"
                         + string.Join("\n", Locale.Select(a => a.Key)),
                    replyToMessageId: args.Message.MessageId);
                return false;
            }
            else if (cmd.Parameters[0] == "all")
            {
                await ctx.Client.SendTextMessageAsync(
                    args.Message.Chat.Id,
                    currentLocale["jetkarmabot.changelocale.errorall"],
                    replyToMessageId: args.Message.MessageId);
                return false;
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
                    await ctx.Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.changelocale.toomany"] + "\n" + string.Join("\n", (e.Data["LocaleNames"] as Locale[]).Select(x => x.Name)),
                        replyToMessageId: args.Message.MessageId);
                    return false;
                }
            (await db.Chats.FindAsync(args.Message.Chat.Id)).Locale = localeId;
            log.Debug($"Changed language of chat {args.Message.Chat.Id} to {localeId}");

            currentLocale = Locale[localeId];

            await ctx.Client.SendTextMessageAsync(
                    args.Message.Chat.Id,
(currentLocale.HasNote ? currentLocale["jetkarmabot.changelocale.beforenote"] + currentLocale.Note + "\n" : "")
                    + currentLocale["jetkarmabot.changelocale.justchanged"],
                    replyToMessageId: args.Message.MessageId);
            return true;
        }

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
