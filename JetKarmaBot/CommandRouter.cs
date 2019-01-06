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

            if (CommandString.TryParse(text, out var cmd))
            {
                if (cmd.UserName != null && cmd.UserName != BotUser.Username) // directed not at us!
                    return false;

                try
                {
                    if (commands.ContainsKey(cmd.Command))
                        return commands[cmd.Command].Execute(cmd, args);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
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
