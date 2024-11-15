using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using NLog;

namespace JetKarmaBot;

public class Localization : IReadOnlyDictionary<string, Locale>
{
    private Dictionary<string, Locale> locales = new Dictionary<string, Locale>();

    public Localization(IContainer c)
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
        // Try to find as locale prefix
        IEnumerable<Locale> matchinglocales = locales.Values.Where(x => x.Name.StartsWith(name + "-"));
        if (matchinglocales.Count() > 1)
        {
            LocalizationException l = new LocalizationException("Too many locales");
            l.Data["LocaleNames"] = matchinglocales.ToArray();
            throw l;
        }
        else if (matchinglocales.Count() == 1)
            return matchinglocales.First();
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
public class Locale : IReadOnlyDictionary<string, string>
{
    private Dictionary<string, string> locale;
    private string localeName;
    private string[] commonNames;
    private string note;

    public Locale(JObject locale, string localeName)
    {
        this.locale = locale.Property("strings").Value.ToObject<Dictionary<string, string>>();
        this.localeName = localeName;
        this.commonNames = locale.Property("names").Value.ToObject<string[]>();
        if (locale.ContainsKey("note")) this.note = locale.Property("note").Value.ToObject<string>();
    }
    public string[] CommonNames => commonNames;
    public string Name => localeName;
    public bool HasNote => note != null;

    public string Note => note;

    public IEnumerable<string> Keys => ((IReadOnlyDictionary<string, string>)locale).Keys;

    public IEnumerable<string> Values => ((IReadOnlyDictionary<string, string>)locale).Values;

    public int Count => ((IReadOnlyDictionary<string, string>)locale).Count;

    public string this[string name] => locale.ContainsKey(name) ? locale[name] : "Unmapped locale key: " + name;

    public bool ContainsKey(string key)
    {
        return ((IReadOnlyDictionary<string, string>)locale).ContainsKey(key);
    }

    public bool TryGetValue(string key, out string value)
    {
        return ((IReadOnlyDictionary<string, string>)locale).TryGetValue(key, out value);
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return ((IReadOnlyDictionary<string, string>)locale).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IReadOnlyDictionary<string, string>)locale).GetEnumerator();
    }
}
[System.Serializable]
public class LocalizationException : Exception
{
    public LocalizationException() { }
    public LocalizationException(string message) : base(message) { }
    public LocalizationException(string message, Exception inner) : base(message, inner) { }
    protected LocalizationException(
        SerializationInfo info,
        StreamingContext context) : base(info, context) { }
}