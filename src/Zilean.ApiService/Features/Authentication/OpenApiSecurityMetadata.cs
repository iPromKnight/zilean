namespace Zilean.ApiService.Features.Authentication;

public class OpenApiSecurityMetadata(string securityScheme)
{
    public string SecurityScheme { get; } = securityScheme;
}
