using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace JetKarmaBot.Commands
{
    public class EchoCommand : IChatCommand
    {
        ITelegramBotClient m_client;
        public EchoCommand(ITelegramBotClient client)
        {
            m_client = client;
        }
        public IReadOnlyCollection<string> Names => new[] { "echo" };

        public bool Execute(object sender, MessageEventArgs args)
        {
            m_client.SendTextMessageAsync(args.Message.Chat.Id, args.Message.Text);
            return true;
        }
    }
}
