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
        private string currentFile;
        private Dictionary<string, string> currentLocalization;

        [Inject]
        public Localization(Config cfg)
        {
            Log("Initializing...");
            currentFile = cfg.Language;
            if (string.IsNullOrEmpty(currentFile)) currentFile = "en-US";
            Log("Language is " + currentFile);
            if (!Directory.Exists("lang"))
                Directory.CreateDirectory("lang");

            if (!File.Exists("lang/" + currentFile + ".json") && currentFile != "en-US")
            {
                Log("Language " + currentFile + " not found, falling back to en-US");
                currentFile = "en-US";
            }
            if (!File.Exists("lang/" + currentFile + ".json"))
            {
                Log("Language en-US doesn't exist! Making empty localization");
                currentLocalization = new Dictionary<string, string>();
            }
            else
            {
                currentLocalization = JObject.Parse(File.ReadAllText("lang/" + currentFile + ".json")).ToObject<Dictionary<string, string>>();
                Log("Loaded " + currentFile);
            }
            Log("Initialized!");
        }

        public string this[string name]
        {
            get => GetString(name);
        }
        public string GetString(string name)
        {
            if (!currentLocalization.ContainsKey(name))
            {
                Log(name + " doesn't exist in this localization");
                currentLocalization[name] = "unknown";
                File.WriteAllText("lang/" + currentFile + ".json", JObject.FromObject(currentLocalization).ToString());
                return "unknown";
            }
            else
            {
                return currentLocalization[name];
            }
        }
        void Log(string Message) => Console.WriteLine($"[{nameof(Localization)}]: {Message}");
    }
}
