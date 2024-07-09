namespace Zilean.DmmScraper.Features.PythonSupport;

[Obsolete("Obsolete")]
public class NoopFormatter : IFormatter
{
    public object Deserialize(Stream s) => throw new NotImplementedException();
    public void Serialize(Stream s, object o) {}

    public SerializationBinder? Binder { get; set; }
    public StreamingContext Context { get; set; }
    [Obsolete("Obsolete")]
    public ISurrogateSelector? SurrogateSelector { get; set; }
}
