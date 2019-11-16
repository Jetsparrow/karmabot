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
    public class VerbCommandRouter : ICommandRouter
    {
        Dictionary<string, IChatCommand> commands = new Dictionary<string, IChatCommand>();
        [Inject] private Logger log;
        public string SuperCommand { get; set; }
        public string Prefix => SuperRouter.Prefix + SuperCommand + " ";
        public ICommandRouter SuperRouter { get; set; }

        public Task<bool> Process(ICommandRouter route, CommandString cs, MessageEventArgs args)
        {
            log.Debug($"(verb for {SuperCommand}) Processing verb");
            if (cs.Parameters.Length < 1)
            {
                log.Debug($"(verb for {SuperCommand}) too few arguments");
                return Task.FromResult(false);
            }
            CommandString ncs = new CommandString(cs.Parameters[0], cs.Parameters.Skip(1).ToArray());
            try
            {
                if (commands.ContainsKey(ncs.Command))
                {
                    log.Debug($"(verb for {SuperCommand}) Handling via {commands[ncs.Command].GetType().Name}");
                    return commands[ncs.Command].Execute(this, ncs, args);
                }
            }
            catch (Exception e)
            {
                log.Error($"(verb for {SuperCommand}) Error while handling verb {ncs.Command}!");
                log.Error(e);
                return Task.FromResult(true); //Don't trigger message
            }

            return Task.FromResult(false);
        }

        public void Add(IChatCommand c)
        {
            log.ConditionalTrace($"(verb for {SuperCommand}) Adding command {c.GetType().Name}");
            foreach (var name in c.Names)
            {
                log.ConditionalTrace($"(verb for {SuperCommand}) Mounting {c.GetType().Name} to {name}");
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
                    build = build + Prefix + names[i] + "\n";
                }
                build += "<a href=\"http://example.com\">" + Prefix + names[names.Count - 1] + "</a> " + string.Join(" ", c.Arguments.Select(x => (!x.Required ? "[" : "") + x.Name + (!x.Required ? "]" : ""))) + " <i>" + getLocalizedCMDDesc(c, loc) + "</i>";
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
                build = build + Prefix + names[i] + "\n";
            }
            build += Prefix + names[names.Count - 1] + " " + string.Join(" ", c.Arguments.Select(x => (!x.Required ? "[" : "") + x.Name + (!x.Required ? "]" : ""))) + " <i>" + getLocalizedCMDDesc(c, loc) + "</i>\n";
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