namespace Zilean.ApiService.Features.Torrents;

public class ErrorResponse(string message)
{
    [JsonPropertyName("message")]
    public string Message { get; } = message;
}
