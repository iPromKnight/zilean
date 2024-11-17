using System.Security.Cryptography;

namespace Zilean.Shared.Features.Utilities;

public static partial class Parsing
{
    [GeneratedRegex(
        @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
        RegexOptions.Compiled)]
    private static partial Regex InvalidXmlChars();

    [GeneratedRegex(@"^(?:tt)?(\d{1,8})$", RegexOptions.Compiled)]
    private static partial Regex ImdbIdRegex();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex SpaceRegex();
    [GeneratedRegex(@"(?i)\b(?:a|the|and|of|in|on|with|to|for|by|is|it)\b", RegexOptions.Compiled)]
    private static partial Regex StopWordRegex();
    [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
    private static partial Regex SpaceRemovalRegex();

    public static string NormalizeSpace(string s) => s?.Trim() ?? string.Empty;

    public static string NormalizeMultiSpaces(string s) =>
        SpaceRegex().Replace(NormalizeSpace(s), " ");
    private static string NormalizeNumber(string s, bool isInt = false)
    {
        var valStr = new string(s.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());

        valStr = valStr.Trim().Replace("-", "0");

        if (isInt)
        {
            if (valStr.Contains(',') && valStr.Contains('.'))
            {
                return valStr;
            }

            valStr = valStr.Length == 0 ? "0" : valStr.Replace(".", ",");

            return valStr;
        }

        valStr = valStr.Length == 0 ? "0" : valStr.Replace(",", ".");

        if (valStr.Count(c => c == '.') > 1)
        {
            var lastOcc = valStr.LastIndexOf('.');
            valStr = valStr[..lastOcc].Replace(".", string.Empty) + valStr[lastOcc..];
        }

        return valStr;
    }

    public static Uri? GetMagnetLink(string? infohash) =>
        string.IsNullOrWhiteSpace(infohash) ? null : new Uri($"magnet:?xt=urn:btih:{infohash}");

    public static Guid CreateGuidFromInfohash(string? infohash)
    {
        if (string.IsNullOrEmpty(infohash) || infohash.Length != 40)
        {
            throw new ArgumentException("Infohash must be a 40-character hexadecimal string.", nameof(infohash));
        }

        using var hasher = SHA256.Create();
        byte[] hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(infohash));
        return new Guid(new Span<byte>(hash, 0, 16));
    }


    public static string RemoveInvalidXmlChars(string text) =>
        string.IsNullOrEmpty(text) ? "" : InvalidXmlChars().Replace(text, "");

    public static double CoerceDouble(string str) =>
        double.Parse(NormalizeNumber(str), NumberStyles.Any, CultureInfo.InvariantCulture);

    public static float CoerceFloat(string str) =>
        float.Parse(NormalizeNumber(str), NumberStyles.Any, CultureInfo.InvariantCulture);

    public static int CoerceInt(string str) =>
        int.Parse(NormalizeNumber(str, true), NumberStyles.Any, CultureInfo.InvariantCulture);

    public static long CoerceLong(string str) =>
        long.Parse(NormalizeNumber(str, true), NumberStyles.Any, CultureInfo.InvariantCulture);

    public static bool TryCoerceDouble(string str, out double result) => double.TryParse(NormalizeNumber(str), NumberStyles.Any,
        CultureInfo.InvariantCulture, out result);

    public static bool TryCoerceFloat(string str, out float result) => float.TryParse(NormalizeNumber(str), NumberStyles.Any,
        CultureInfo.InvariantCulture, out result);

    public static bool TryCoerceInt(string str, out int result) => int.TryParse(NormalizeNumber(str, true), NumberStyles.Any,
        CultureInfo.InvariantCulture, out result);

    public static bool TryCoerceLong(string str, out long result) => long.TryParse(NormalizeNumber(str, true), NumberStyles.Any,
        CultureInfo.InvariantCulture, out result);

    public static string GetArgumentFromQueryString(string? url, string? argument)
    {
        if (url == null || argument == null)
        {
            return null;
        }

        var qsStr = url.Split(['?'], 2)[1];
        qsStr = qsStr.Split(['#'], 2)[0];
        var qs = QueryHelpers.ParseQuery(qsStr);
        return qs[argument].FirstOrDefault();
    }

    public static long? GetLongFromString(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return null;
        }

        var extractedLong = string.Empty;

        foreach (var c in str)
        {
            if (c is < '0' or > '9')
            {
                if (extractedLong.Length > 0)
                {
                    break;
                }

                continue;
            }

            extractedLong += c;
        }

        return CoerceLong(extractedLong);
    }

    public static int? GetImdbId(string? value)
    {
        if (value == null)
        {
            return null;
        }

        var match = ImdbIdRegex().Match(value);

        return !match.Success ? null : int.Parse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture);
    }

    public static string GetFullImdbId(string? value)
    {
        var imdbId = GetImdbId(value);

        return imdbId is null or 0 ? null : $"tt{imdbId.GetValueOrDefault():D7}";
    }

    // ex: " 3.5  gb   " -> "3758096384" , "3,5GB" -> "3758096384" ,  "296,98 MB" -> "311406100.48" , "1.018,29 MB" -> "1067754455.04"
    // ex:  "1.018.29mb" -> "1067754455.04" , "-" -> "0" , "---" -> "0"
    public static long GetBytes(string str)
    {
        var valStr = new string(str.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
        valStr = (valStr.Length == 0) ? "0" : valStr.Replace(",", ".");
        if (valStr.Count(c => c == '.') > 1)
        {
            var lastOcc = valStr.LastIndexOf('.');
            valStr = valStr[..lastOcc].Replace(".", string.Empty) + valStr[lastOcc..];
        }

        var unit = new string(str.Where(char.IsLetter).ToArray());
        var val = CoerceFloat(valStr);
        return GetBytes(unit, val);
    }

    public static long GetBytes(string unit, float value)
    {
        unit = unit.Replace("i", "").ToLowerInvariant();

        return unit.Contains("kb")
            ? BytesFromKB(value)
            : unit.Contains("mb")
            ? BytesFromMB(value)
            : unit.Contains("gb") ? BytesFromGB(value) : unit.Contains("tb") ? BytesFromTB(value) : (long)value;
    }

    public static long BytesFromTB(float tb) => BytesFromGB(tb * 1024f);

    public static long BytesFromGB(float gb) => BytesFromMB(gb * 1024f);

    public static long BytesFromMB(float mb) => BytesFromKB(mb * 1024f);

    public static long BytesFromKB(float kb) => (long)(kb * 1024f);

    public static string CleanQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return query;
        }

        var cleanedQuery = StopWordRegex().Replace(query, "");
        cleanedQuery = SpaceRemovalRegex().Replace(cleanedQuery, " ").Trim();

        return cleanedQuery;
    }
}
