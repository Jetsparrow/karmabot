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
        private Dictionary<string, Locale> locales = new Dictionary<string, Locale>();

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
                    locales[langKey] = new Locale(JObject.Parse(File.ReadAllText(langFilePath)), langKey);
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
                return locales[locale];
            }
        }
        public Locale FindByCommonName(string name)
        {
            foreach (Locale l in locales.Values)
            {
                if (l.CommonNames.Contains(name))
                {
                    return l;
                }
            }
            return null;
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
            private string[] commonNames;

            public Locale(JObject locale, string localeName)
            {
                this.locale = locale.Property("strings").Value.ToObject<Dictionary<string, string>>();
                this.localeName = localeName;
                this.commonNames = locale.Property("names").Value.ToObject<string[]>();
            }
            public string[] CommonNames => commonNames;
            public string Name => localeName;
            public string this[string name] => locale.ContainsKey(name) ? locale[name] : "unknown";
        }
    }
}