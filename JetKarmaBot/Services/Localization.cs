using System;
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

        public string this[string name, string locale]
        {
            get => GetString(name, locale);
        }
        public string GetString(string name, string locale)
        {
            if (!locales[locale].ContainsKey(name))
            {
                Log(name + " doesn't exist in this localization");
                locales[locale][name] = "unknown";
                File.WriteAllText("lang/" + locale + ".json", JObject.FromObject(locales[locale]).ToString());
                return "unknown";
            }
            else
            {
                return locales[locale][name];
            }
        }
        void Log(string Message) => Console.WriteLine($"[{nameof(Localization)}]: {Message}");
    }
}
