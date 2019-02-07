using System.Collections.Generic;
using Telegram.Bot.Args;
using Perfusion;
using JetKarmaBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace JetKarmaBot.Commands
{
    public class HelpCommand : IChatCommand
    {
        [Inject] KarmaContextFactory Db;
        [Inject] TelegramBotClient Client { get; set; }
        [Inject] Localization Locale { get; set; }
        ChatCommandRouter Router;
        public IReadOnlyCollection<string> Names => new[] { "help" };

        public string Description => "Displays help text for all(one) command(s)";
        public string DescriptionID => "jetkarmabot.help.help";

        public IReadOnlyCollection<ChatCommandArgument> Arguments => new ChatCommandArgument[] {
            new ChatCommandArgument() {
                Name="command",
                Required=false,
                Type=ChatCommandArgumentType.String,
                Description="The command to return help text for. If empty shows all commands.",
                DescriptionID="jetkarmabot.help.commandhelp"
            }
         };

        public bool Execute(CommandString cmd, MessageEventArgs args)
        {
            using (var db = Db.GetContext())
            {
                var currentLocale = Locale[db.Chats.Find(args.Message.Chat.Id).Locale];
                if (cmd.Parameters.Length < 1)
                {
                    Client.SendTextMessageAsync(
                            args.Message.Chat.Id,
                            Router.GetHelpText(currentLocale),
                            replyToMessageId: args.Message.MessageId,
                            parseMode: ParseMode.Html);
                    return true;
                }
                else
                {
                    Client.SendTextMessageAsync(
                            args.Message.Chat.Id,
                            Router.GetHelpTextFor(cmd.Parameters[0], currentLocale),
                            replyToMessageId: args.Message.MessageId,
                            parseMode: ParseMode.Html);
                    return true;
                }
            }
        }
        public HelpCommand(ChatCommandRouter router)
        {
            Router = router;
        }
    }
}
