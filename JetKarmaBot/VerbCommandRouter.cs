using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetKarmaBot.Commands;
using NLog;
using Perfusion;
using Telegram.Bot.Args;

namespace JetKarmaBot
{
    public class VerbCommandRouter
    {
        Dictionary<string, IChatCommand> commands = new Dictionary<string, IChatCommand>();
        [Inject] private Logger log;
        string superCommand;
        public VerbCommandRouter(string supercommand)
        {
            superCommand = supercommand;
        }

        public Task<bool> Process(CommandString cs, MessageEventArgs args)
        {
            log.Debug($"(verb for {superCommand}) Processing verb");
            if (cs.Parameters.Length < 1)
            {
                log.Debug($"(verb for {superCommand}) too few arguments");
                return Task.FromResult(false);
            }
            CommandString ncs = new CommandString(cs.Parameters[0], cs.Parameters.Skip(1).ToArray());
            try
            {
                if (commands.ContainsKey(ncs.Command))
                {
                    log.Debug($"(verb for {superCommand}) Handling via {commands[ncs.Command].GetType().Name}");
                    return commands[ncs.Command].Execute(ncs, args);
                }
            }
            catch (Exception e)
            {
                log.Error($"(verb for {superCommand}) Error while handling verb {ncs.Command}!");
                log.Error(e);
                return Task.FromResult(true); //Don't trigger message
            }

            return Task.FromResult(false);
        }

        public void Add(IChatCommand c)
        {
            log.ConditionalTrace($"(verb for {superCommand}) Adding command {c.GetType().Name}");
            foreach (var name in c.Names)
            {
                log.ConditionalTrace($"(verb for {superCommand}) Mounting {c.GetType().Name} to {name}");
                if (commands.ContainsKey(name))
                    throw new Exception($"command collision for name {name}, commands {commands[name].GetType()} and {c.GetType()}");
                commands[name] = c;
            }
        }

        internal string GetHelpText(Locale loc)
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

        internal string GetHelpTextFor(string commandname, Locale loc)
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
    }
}