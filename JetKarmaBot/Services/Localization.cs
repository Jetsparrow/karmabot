using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLog;
using Perfusion;

namespace JetKarmaBot
{
    public class Localization : IReadOnlyDictionary<string, Locale>
    {
        private Dictionary<string, Locale> locales = new Dictionary<string, Locale>();

        [Inject]
        public Localization(Container c)
        {
            c.ResolveObject(this);
            log.Info("Initializing...");
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
                    log.Debug("Found " + langName);
                }
                catch (Exception e)
                {
                    log.Error($"Error while parsing {langFilePath}!");
                    log.Error(e);
                }
            }

            if (locales.Any())
                log.Info("Initialized!");
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

        [Inject]
        private Logger log;

        public Locale FindByCommonName(string name)
        {
            log.ConditionalTrace("Trying to find locale " + name);
            foreach (Locale l in locales.Values)
            {
                if (l.CommonNames.Contains(name))
                {
                    log.ConditionalTrace("Found locale " + l.Name);
                    return l;
                }
            }
            log.Warn("Failed to find locale " + name);
            return null;
        }
        public bool ContainsLocale(string locale)
        {
            locale = locale.ToLowerInvariant();
            return locales.ContainsKey(locale);
        }

        void Log(Exception e) => Console.WriteLine(e);

        public IEnumerable<string> Keys => locales.Keys;

        public IEnumerable<Locale> Values => locales.Values;

        public int Count => locales.Count;

        public bool ContainsKey(string key) => locales.ContainsKey(key.ToLower());

        public bool TryGetValue(string key, out Locale value)
        {
            return locales.TryGetValue(key.ToLower(), out value);
        }

        public IEnumerator<KeyValuePair<string, Locale>> GetEnumerator()
        {
            return locales.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    }
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