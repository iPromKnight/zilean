namespace Zilean.Shared.Features.Utilities;

public static class ApiKey
{
    public static string Generate() => $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
}
