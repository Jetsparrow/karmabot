using JetKarmaBot.Commands;
using NLog;
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
        private static Logger log = LogManager.GetCurrentClassLogger();

        public ChatCommandRouter(User botUser)
        {
            BotUser = botUser;
        }

        public bool Execute(object sender, MessageEventArgs args)
        {
            log.Debug("Message received");
            var text = args.Message.Text;
            if (CommandString.TryParse(text, out var cmd))
            {
                if (cmd.UserName != null && cmd.UserName != BotUser.Username)
                {
                    // directed not at us!
                    log.Debug("Message not directed at us");
                    return false;
                }

                try
                {
                    if (commands.ContainsKey(cmd.Command))
                    {
                        log.Debug($"Handling message via {commands[cmd.Command].GetType().Name}");
                        return commands[cmd.Command].Execute(cmd, args);
                    }
                }
                catch (Exception e)
                {
                    log.Error($"Error while handling command {cmd.Command}!");
                    log.Error(e);
                }
            }


            return false;
        }

        public void Add(IChatCommand c)
        {
            log.ConditionalTrace($"Adding command {c.GetType().Name}");
            foreach (var name in c.Names)
            {
                log.ConditionalTrace($"Mounting {c.GetType().Name} to {name}");
                if (commands.ContainsKey(name))
                    throw new Exception($"command collision for name {name}, commands {commands[name].GetType()} and {c.GetType()}");
                commands[name] = c;
            }
        }

        Dictionary<string, IChatCommand> commands = new Dictionary<string, IChatCommand>();
    }
}
