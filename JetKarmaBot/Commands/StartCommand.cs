using System.Collections.Generic;
using Telegram.Bot.Args;
using Perfusion;

namespace JetKarmaBot.Commands
{
    public class StartCommand : IChatCommand
    {
        [Inject(true)]Db Db;

        public IReadOnlyCollection<string> Names => new[] { "start" };

        public bool Execute(object sender, MessageEventArgs args)
        {
            Db.AddChat(new Db.Chat { ChatId = args.Message.Chat.Id });
            Db.AddUser(new Db.User { UserId = args.Message.From.Id });
            return true;
        }
    }
}
