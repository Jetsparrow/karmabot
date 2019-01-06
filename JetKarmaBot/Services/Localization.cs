using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            string langsFolder = "lang";
            if (!Directory.Exists(langsFolder))
                Directory.CreateDirectory(langsFolder);

            foreach (string langFilePath in Directory.EnumerateFiles(langsFolder, "*.json"))
            {
                try
                {
                    string langName = Path.GetFileNameWithoutExtension(langFilePath);
                    string langKey = langName.ToLowerInvariant();
                    locales[langKey] = JObject.Parse(File.ReadAllText(langFilePath)).ToObject<Dictionary<string, string>>();
                    Log("Found " + langName);
                }
                catch (Exception e)
                {
                    Log($"Error while parsing {langFilePath}!");
                    Log(e);
                }
            }

            if (locales.Any())
                Log("Initialized!");
            else
                throw new FileNotFoundException($"No locales found in {langsFolder}!");
        }

        public Locale this[string locale]
        {
            get
            {
                locale = locale.ToLowerInvariant();
                return new Locale(locales[locale], locale);
            }
        }
        public bool ContainsLocale(string locale)
        {
            locale = locale.ToLowerInvariant();
            return locales.ContainsKey(locale);
        }

        void Log(string Message)
            => Console.WriteLine($"[{nameof(Localization)}]: {Message}");

        void Log(Exception e) => Console.WriteLine(e);

        public class Locale
        {
            private Dictionary<string, string> locale;
            private string localeName;
            public Locale(Dictionary<string, string> locale, string localeName)
            {
                this.locale = locale;
                this.localeName = localeName;
            }
            public string this[string name] => locale.ContainsKey(name) ? locale[name] : "unknown";
        }
    }
}