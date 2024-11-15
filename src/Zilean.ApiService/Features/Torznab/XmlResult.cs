namespace Zilean.ApiService.Features.Torznab;

public static class StreamManager
{
    public static RecyclableMemoryStreamManager Instance { get; } = new();
}

public class XmlResult<T>(T result, int statusCode) : IResult
{
    private static readonly XmlSerializer _serializer = new(typeof(T));

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.ContentType = "application/xml";
        httpContext.Response.StatusCode = statusCode;

        if (result is string xmlString)
        {
            // Handle string results directly to avoid wrapping in a <string> tag
            await httpContext.Response.WriteAsync(xmlString);
        }
        else
        {
            // Serialize non-string objects to XML
            await using var ms = StreamManager.Instance.GetStream();
            _serializer.Serialize(ms, result);

            ms.Position = 0;
            await ms.CopyToAsync(httpContext.Response.Body);
        }
    }
}
