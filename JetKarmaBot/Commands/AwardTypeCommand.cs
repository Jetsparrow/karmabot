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

        public VerbCommandRouter Router;
        [Inject] TelegramBotClient Client { get; set; }
        [Inject] Localization Locale { get; set; }
        [Inject] KarmaContextFactory Db { get; set; }

        public AwardTypeCommand(IContainer c, VerbCommandRouter r)
        {
            Router = r;
            Router.SuperCommand = "at";
            r.Add(c.GetInstance<AwardTypeManage.CreateCommand>());
            r.Add(c.GetInstance<AwardTypeManage.RemoveCommand>());
            r.Add(c.GetInstance<AwardTypeManage.SetParameterCommand>());
            r.Add(c.GetInstance<HelpCommand>());
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

        public async Task<bool> Execute(ICommandRouter route, CommandString cmd, MessageEventArgs args)
        {
            Router.SuperRouter = route;
            using (var db = Db.GetContext())
            {
                var currentLocale = Locale[(await db.Chats.FindAsync(args.Message.Chat.Id)).Locale];
                if (!await Router.Process(route, cmd, args))
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