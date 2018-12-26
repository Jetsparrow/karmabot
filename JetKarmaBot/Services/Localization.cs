using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Perfusion;

namespace JetKarmaBot
{
    public class Localization
    {
        private Dictionary<string, Dictionary<string, string>> locales = new Dictionary<string, Dictionary<string, string>>();

        [Inject]
        public Localization(Config cfg)
        {
            Log("Initializing...");
            if (!Directory.Exists("lang"))
                Directory.CreateDirectory("lang");

            foreach (string lang in Directory.EnumerateFiles("lang"))
            {
                string langname = Path.GetFileName(lang).Split(".")[0];
                Log("Found " + langname);
                locales[langname] = JObject.Parse(File.ReadAllText(lang)).ToObject<Dictionary<string, string>>();
            }
            Log("Initialized!");
        }

        public Locale this[string locale]
        {
            get => new Locale(locales[locale], locale);
        }
        void Log(string Message) => Console.WriteLine($"[{nameof(Localization)}]: {Message}");
        public class Locale
        {
            private Dictionary<string, string> locale;
            private string localeName;
            public Locale(Dictionary<string, string> locale, string localeName)
            {
                this.locale = locale;
                this.localeName = localeName;
            }
            public string this[string name]
            {
                get
                {
                    if (!locale.ContainsKey(name))
                    {
                        return "unknown";
                    }
                    else
                    {
                        return locale[name];
                    }
                }
            }
        }
    }
}