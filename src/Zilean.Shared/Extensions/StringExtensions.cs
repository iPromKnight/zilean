namespace Zilean.Shared.Extensions;

public static class StringExtensions
{
    public static bool ContainsIgnoreCase(this string? source, string toCheck) =>
        source.Contains(toCheck, StringComparison.OrdinalIgnoreCase);

    public static bool ContainsIgnoreCase(this IEnumerable<string>? source, string toCheck) =>
        source.Any(s => s.Contains(toCheck, StringComparison.OrdinalIgnoreCase));
    
    public static bool IsNullOrWhiteSpace(this string? source) =>
        string.IsNullOrWhiteSpace(source);
}
