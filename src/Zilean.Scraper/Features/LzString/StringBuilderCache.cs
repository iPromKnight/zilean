namespace Zilean.Scraper.Features.LzString;

public static class StringBuilderCache
{
    [ThreadStatic]
    private static StringBuilder? _cachedInstance;

    public static StringBuilder Acquire(int capacity = 16)
    {
        if (capacity > 360)
        {
            return new StringBuilder(capacity);
        }

        var sb = _cachedInstance;

        if (sb == null || capacity > sb.Capacity)
        {
            return new StringBuilder(capacity);
        }

        _cachedInstance = null;
        sb.Clear();

        return sb;
    }

    public static string GetStringAndRelease(StringBuilder sb)
    {
        string result = sb.ToString();
        Release(sb);
        return result;
    }

    private static void Release(StringBuilder sb)
    {
        if (sb.Capacity <= 360)
        {
            _cachedInstance = sb;
        }
    }
}
