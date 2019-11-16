using JetKarmaBot.Commands;
using JetKarmaBot.Services;
using Microsoft.EntityFrameworkCore;
using NLog;
using Perfusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace JetKarmaBot
{
    public class ChatCommandRouter : ICommandRouter
    {
        public Telegram.Bot.Types.User Me { get; private set; }
        [Inject] private Logger log;
        [Inject] private TelegramBotClient Client { get; set; }

        public string Prefix => "/";

        public async Task Start()
        {
            Me = await Client.GetMeAsync();
        }

        public Task<bool> Execute(object sender, MessageEventArgs args)
        {
            log.Debug("Message received");
            var text = args.Message.Text;
            if (CommandString.TryParse(text, out var cmd))
            {
                if (cmd.UserName != null && cmd.UserName != Me.Username)
                {
                    // directed not at us!
                    log.Debug("Message not directed at us");
                    return Task.FromResult(false);
                }

                try
                {
                    if (commands.ContainsKey(cmd.Command))
                    {
                        log.Debug($"Handling message via {commands[cmd.Command].GetType().Name}");
                        return commands[cmd.Command].Execute(this, cmd, args);
                    }
                }
                catch (Exception e)
                {
                    log.Error($"Error while handling command {cmd.Command}!");
                    log.Error(e);
                }
            }

            return Task.FromResult(false);
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

        public string GetHelpText(Locale loc)
        {
            List<string> pieces = new List<string>();
            foreach (IChatCommand c in commands.Values.Distinct())
            {
                string build = "";
                List<string> names = c.Names.ToList();
                for (int i = 0; i < names.Count - 1; i++)
                {
                    build = build + "/" + names[i] + "\n";
                }
                build += "/" + names[names.Count - 1] + " " + string.Join(" ", c.Arguments.Select(x => (!x.Required ? "[" : "") + x.Name + (!x.Required ? "]" : ""))) + " <i>" + getLocalizedCMDDesc(c, loc) + "</i>";
                pieces.Add(build);
            }
            return string.Join("\n", pieces);
        }

        public string GetHelpTextFor(string commandname, Locale loc)
        {
            IChatCommand c = commands[commandname];
            string build = "";
            List<string> names = c.Names.ToList();
            for (int i = 0; i < names.Count - 1; i++)
            {
                build = build + "/" + names[i] + "\n";
            }
            build += "/" + names[names.Count - 1] + " " + string.Join(" ", c.Arguments.Select(x => (!x.Required ? "[" : "") + x.Name + (!x.Required ? "]" : ""))) + " <i>" + getLocalizedCMDDesc(c, loc) + "</i>\n";
            build += string.Join("\n", c.Arguments.Select(ca => (!ca.Required ? "[" : "") + ca.Name + (!ca.Required ? "]" : "") + ": <i>" + getLocalizedCMDArgDesc(ca, loc) + "</i>"));
            return build;
        }

        private string getLocalizedCMDDesc(IChatCommand cmd, Locale loc)
        {
            if (loc.ContainsKey(cmd.DescriptionID)) return loc[cmd.DescriptionID];
            else return cmd.Description;
        }
        private string getLocalizedCMDArgDesc(ChatCommandArgument arg, Locale loc)
        {
            if (loc.ContainsKey(arg.DescriptionID)) return loc[arg.DescriptionID];
            else return arg.Description;
        }

        Dictionary<string, IChatCommand> commands = new Dictionary<string, IChatCommand>();
    }
}
