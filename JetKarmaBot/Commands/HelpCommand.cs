using System.Collections.Generic;
using Perfusion;
using JetKarmaBot.Services.Handling;
using Telegram.Bot.Types.Enums;
using System.Threading.Tasks;
using JetKarmaBot.Models;

namespace JetKarmaBot.Commands
{
    public class HelpCommand : IChatCommand
    {
        [Inject] Localization Locale { get; set; }
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

        public async Task<bool> Execute(RequestContext ctx)
        {
            var currentLocale = ctx.GetFeature<Locale>();
            var router = ctx.GetFeature<ChatCommandRouter.Feature>().Router;
            if (ctx.Command.Parameters.Length < 1)
            {
                await ctx.SendMessage(router.GetHelpText(currentLocale));
                return true;
            }
            else
            {
                await ctx.SendMessage(router.GetHelpTextFor(ctx.Command.Parameters[0], currentLocale));
                return true;
            }
        }
    }
}
