using System.Collections.Generic;
using System.Threading.Tasks;
using JetKarmaBot.Services;
using Perfusion;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace JetKarmaBot.Commands
{
    public class AwardTypeCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "at" };

        public string Description => "Manage custom award types.";

        public string DescriptionID => "jetkarmabot.at.help";

        public ChatCommandRouter VerbRouter;
        [Inject] TelegramBotClient Client { get; set; }
        [Inject] Localization Locale { get; set; }
        [Inject] KarmaContextFactory Db { get; set; }
        [Inject] IContainer C { get; set; }

        public void OnMount()
        {
            VerbRouter = C.ResolveObject(new ChatCommandRouter());
            VerbRouter.SuperCommand = "at";
            VerbRouter.SuperRouter = Router;
            VerbRouter.Me = Router.Me;
            VerbRouter.Add(C.GetInstance<AwardTypeManage.CreateCommand>());
            VerbRouter.Add(C.GetInstance<AwardTypeManage.RemoveCommand>());
            VerbRouter.Add(C.GetInstance<AwardTypeManage.SetParameterCommand>());
            VerbRouter.Add(C.GetInstance<HelpCommand>());
        }

        public IReadOnlyCollection<ChatCommandArgument> Arguments => new[] {
            new ChatCommandArgument() {
                Name="verb",
                Required=true,
                Type=ChatCommandArgumentType.String,
                Description="The action to perform.",
                DescriptionID="jetkarmabot.at.verbhelp"
            }
        };

        public ICommandRouter Router { get; set; }

        public async Task<bool> Execute(CommandString cmd, MessageEventArgs args)
        {
            VerbRouter.SuperRouter = Router;
            using (var db = Db.GetContext())
            {
                var currentLocale = Locale[(await db.Chats.FindAsync(args.Message.Chat.Id)).Locale];
                if (!await VerbRouter.Execute(cmd, args))
                {
                    await Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.at.err"],
                        replyToMessageId: args.Message.MessageId);
                }
                return true;
            }
        }
    }
}