namespace JetKarmaBot;

public static class IReadOnlyDictionaryExtensions
{
    public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key)
    {
        TValue res = default;
        if (key != null)
            dict.TryGetValue(key, out res);
        return res;
    }
}
