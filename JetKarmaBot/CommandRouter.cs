using JetKarmaBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Args;

namespace JetKarmaBot
{
    class ChatCommandRouter
    {
        Dictionary<string, IChatCommand> commands = new Dictionary<string, IChatCommand>();
        public bool Execute(object sender, MessageEventArgs args)
        {
            var text = args.Message.Text;

            if (CommandString.TryParse(text, out var cs))
            {
                if (commands.ContainsKey(cs.Name))
                    return commands[cs.Name].Execute(sender,args);
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

    }
}
