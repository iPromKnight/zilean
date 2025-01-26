using J2N.IO;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Util;

namespace Zilean.Database.Dtos;

public sealed class LuceneSession : IDisposable
{
    public RAMDirectory? Directory { get; } = new();
    public StandardAnalyzer? Analyzer { get; } = new(LuceneVersion.LUCENE_48);
    public IndexWriterConfig? Config { get; private set; }
    public IndexWriter? Writer { get; private set; }

    public static LuceneSession NewInstance()
    {
        var instance = new LuceneSession();

        instance.Config = new(LuceneVersion.LUCENE_48, instance.Analyzer);
        instance.Writer = new(instance.Directory, instance.Config);

        return instance;
    }

    public void Dispose()
    {
        Directory?.Dispose();
        Analyzer?.Dispose();
        Writer?.Dispose();
    }
}
