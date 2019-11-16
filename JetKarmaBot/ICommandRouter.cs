using JetKarmaBot.Commands;

namespace JetKarmaBot
{
    public interface ICommandRouter
    {
        string GetHelpText(Locale loc);
        string GetHelpTextFor(string commandname, Locale loc);
        void Add(IChatCommand c);
        string Prefix { get; }
    }
}