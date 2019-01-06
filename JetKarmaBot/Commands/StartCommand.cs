using System.Collections.Generic;
using Telegram.Bot.Args;
using Perfusion;
using JetKarmaBot.Services;

namespace JetKarmaBot.Commands
{
    public class StartCommand : IChatCommand
    {
        [Inject] KarmaContextFactory Db;

        public IReadOnlyCollection<string> Names => new[] { "start" };

        public bool Execute(CommandString cmd, MessageEventArgs args)
        {
            using (var db = Db.GetContext())
            {
                db.Chats.Add(new Models.Chat { ChatId = args.Message.Chat.Id });
                db.Users.Add(new Models.User { UserId = args.Message.From.Id });
                db.SaveChanges();
                return true;
            }
        }
    }
}
