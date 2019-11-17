using JetKarmaBot.Commands;

namespace JetKarmaBot
{
    public interface ICommandRouter
    {
        Telegram.Bot.Types.User Me { get; set; }
        string GetHelpText(Locale loc);
        string GetHelpTextFor(string commandname, Locale loc);
        void Add(IChatCommand c);
        string Prefix { get; }
    }
}