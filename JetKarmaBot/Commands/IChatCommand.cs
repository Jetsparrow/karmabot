using System.Collections.Generic;
using Telegram.Bot.Args;

namespace JetKarmaBot.Commands
{
    public interface IChatCommand
    {
        IReadOnlyCollection<string> Names { get; }
        string Description { get; }
        IReadOnlyCollection<ChatCommandArgument> Arguments { get; }

        bool Execute(CommandString cmd, MessageEventArgs messageEventArgs);
    }

    public struct ChatCommandArgument
    {
        public string Name;
        public bool Required;
        public ChatCommandArgumentType Type;
        public string Description;
    }

    public enum ChatCommandArgumentType
    {
        Boolean,
        String,
        Integer,
    }
}
