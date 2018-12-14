using System;
using System.Collections.Generic;
using System.Text;

namespace JetKarmaBot.Commands
{
    public class CommandString
    {
        public CommandString(string name, params string[] parameters)
        {
            Name = name;
            Parameters = parameters;
        }

        public string Name { get; }
        public string[] Parameters { get; }

        public static bool TryParse(string s, out CommandString result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(s) || s[0] != '/')
                return false;

            int space = s.IndexOf(' ');
            if (space < 0)
                result = new CommandString(s.Substring(1));
            else
                result = new CommandString(s.Substring(1, space - 1), s.Substring(space).Split(' ', StringSplitOptions.RemoveEmptyEntries));
            return true;
        }

        public static CommandString Parse(string s)
        {
            if (TryParse(s, out var c)) return c;
            throw new ArgumentException($"\"{s}\" is not a command");
        }
    }
}
