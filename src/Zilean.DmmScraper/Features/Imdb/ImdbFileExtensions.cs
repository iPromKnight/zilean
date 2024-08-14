namespace Zilean.DmmScraper.Features.Imdb;

public static class ImdbFileExtensions
{
    public static List<ImdbFile> GetCandidatesForYearRange(this ConcurrentDictionary<int, List<ImdbFile>> imdbFiles, int year)
    {
        var candidates = new List<ImdbFile>();

        for (int y = year - 1; y <= year + 1; y++)
        {
            if (imdbFiles.TryGetValue(y, out var files))
            {
                candidates.AddRange(files);
            }
        }

        return candidates;
    }
}
