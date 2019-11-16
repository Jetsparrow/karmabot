using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Args;

namespace JetKarmaBot.Commands.AwardTypeManage
{
    public class TestCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "test" };

        public string Description => "test";

        public string DescriptionID => "test";

        public IReadOnlyCollection<ChatCommandArgument> Arguments => Array.Empty<ChatCommandArgument>();

        public Task<bool> Execute(ICommandRouter route, CommandString cmd, MessageEventArgs messageEventArgs)
        {
            throw new NotImplementedException();
        }
    }
}