namespace Zilean.Shared.Extensions;

public static class CollectionExtensions
{
    public static IEnumerable<List<TType>> ToChunks<TType>(this List<TType> collection, int batchSize)
    {
        for (int i = 0; i < collection.Count; i += batchSize)
        {
            yield return collection.GetRange(i, Math.Min(batchSize, collection.Count - i));
        }
    }

    public static async IAsyncEnumerable<List<T>> ToChunksAsync<T>(this IAsyncEnumerable<T> source, int size,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (size <= 0)
        {
            throw new ArgumentException("Chunk size must be greater than zero.", nameof(size));
        }

        var batch = new List<T>(size);

        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            batch.Add(item);
            if (batch.Count != size)
            {
                continue;
            }

            yield return batch;
            batch = new List<T>(size);
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }
}
