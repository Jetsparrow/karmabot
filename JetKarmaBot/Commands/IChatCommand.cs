using System.Collections.Generic;
using Telegram.Bot.Args;

namespace JetKarmaBot.Commands
{
    public interface IChatCommand
    {
        IReadOnlyCollection<string> Names { get; }
        bool Execute(object sender, MessageEventArgs messageEventArgs);
    }

}
