using System.Text.Json;

namespace Zilean.Shared.Extensions;

public static class JsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        NumberHandling = JsonNumberHandling.Strict,
    };

    public static string AsJson<T>(this T obj) => JsonSerializer.Serialize(obj, _jsonSerializerOptions);
}
