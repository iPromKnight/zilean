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
}
