using System.Collections.Generic;
using Telegram.Bot.Args;

namespace JetKarmaBot.Commands
{
    public class StartCommand : IChatCommand
    {
        Db m_db;
        public StartCommand(Db db)
        {
            m_db = db;
        }
        public IReadOnlyCollection<string> Names => new[] { "start" };

        public bool Execute(object sender, MessageEventArgs args)
        {
            m_db.AddChat(new Db.Chat { ChatId = args.Message.Chat.Id });
            m_db.AddUser(new Db.User { UserId = args.Message.From.Id });
            return true;
        }
    }
}
