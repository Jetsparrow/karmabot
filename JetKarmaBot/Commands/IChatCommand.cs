using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Args;

namespace JetKarmaBot.Commands
{
    public interface IChatCommand
    {
        IReadOnlyCollection<string> Names { get; }
        string Description { get; }
        string DescriptionID { get; }
        IReadOnlyCollection<ChatCommandArgument> Arguments { get; }

        Task<bool> Execute(ICommandRouter route, CommandString cmd, MessageEventArgs messageEventArgs);
    }

    public struct ChatCommandArgument
    {
        public string Name;
        public bool Required;
        public ChatCommandArgumentType Type;
        public string Description;
        public string DescriptionID;
    }

    public enum ChatCommandArgumentType
    {
        Boolean,
        String,
        Integer,
    }
}
