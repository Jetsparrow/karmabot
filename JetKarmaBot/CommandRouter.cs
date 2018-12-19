using JetKarmaBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace JetKarmaBot
{
    class ChatCommandRouter
    {
        User BotUser { get; }

        public ChatCommandRouter(User botUser)
        {
            BotUser = botUser;
        }

        public bool Execute(object sender, MessageEventArgs args)
        {
            var text = args.Message.Text;
            
            if (CommandString.TryParse(text, out var cs))
            {
                if (cs.UserName != null && cs.UserName != BotUser.Username) // directed not at us!
                    return false;

                if (commands.ContainsKey(cs.Command))
                    return commands[cs.Command].Execute(sender,args);
            }

            return false;
        }

        public void Add(IChatCommand c)
        {
            foreach (var name in c.Names)
            {
                if (commands.ContainsKey(name))
                    throw new Exception($"command collision for name {name}, commands {commands[name].GetType()} and {c.GetType()}");
                commands[name] = c;
            }
        }

        Dictionary<string, IChatCommand> commands = new Dictionary<string, IChatCommand>();
    }
}
