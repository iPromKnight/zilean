namespace Zilean.Shared.Features.Torznab;

public static class TorznabErrorResponse
{
    public static string Create(int code, string description)
    {
        var xdoc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement("error",
                new XAttribute("code", code.ToString()),
                new XAttribute("description", description)
            )
        );

        return xdoc.Declaration + Environment.NewLine + xdoc;
    }
}
