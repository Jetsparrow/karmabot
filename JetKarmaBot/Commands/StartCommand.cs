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
                db.Chat.Add(new Models.Chat { Chatid = args.Message.Chat.Id });
                db.User.Add(new Models.User { Userid = args.Message.From.Id });
                db.SaveChanges();
                return true;
            }
        }
    }
}
