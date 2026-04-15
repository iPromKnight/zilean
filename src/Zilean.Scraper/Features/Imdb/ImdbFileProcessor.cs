namespace Zilean.Scraper.Features.Imdb;

public class ImdbFileProcessor(ILogger<ImdbFileProcessor> logger, IImdbFileService imdbFileService)
{
    private static readonly List<string> _requiredCategories = [
        "movie",
        "tvMovie",
        "tvSeries",
        "tvShort",
        "tvMiniSeries",
        "tvSpecial",
    ];

    public async Task Import(string fileName, CancellationToken cancellationToken)
    {
        logger.LogInformation("Importing Downloaded IMDB Basics data from {FilePath}", fileName);

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = "\t",
            BadDataFound = null,
            MissingFieldFound = null,
            HasHeaderRecord = true,
        };

        using var reader = new StreamReader(fileName);
        using var csv = new CsvReader(reader, csvConfig);

        await csv.ReadAsync();
        csv.ReadHeader();

        await ReadBasicEntries(csv, imdbFileService, cancellationToken);

        await imdbFileService.StoreImdbFiles();

        await imdbFileService.VaccumImdbFilesIndexes(cancellationToken);
    }

    private static async Task ReadBasicEntries(CsvReader csv, IImdbFileService imdbFileService, CancellationToken cancellationToken)
    {
        while (await csv.ReadAsync())
        {
            var category = csv.GetField(1);

            if (!_requiredCategories.Contains(category))
            {
                continue;
            }

            var primaryTitle = GetNullableField(csv.GetField(2));
            var originalTitle = GetNullableField(csv.GetField(3));
            var isAdultSet = int.TryParse(csv.GetField(4), out var adult);
            var yearField = csv.GetField(5);
            var isYearValid = int.TryParse(yearField == @"\N" ? "0" : yearField, out var year);

            var movieData = new ImdbFile
            {
                ImdbId = csv.GetField(0),
                Category = category,
                Title = primaryTitle ?? originalTitle,
                OriginalTitle = originalTitle ?? primaryTitle,
                Adult = isAdultSet && adult == 1,
                Year = isYearValid ? year : 0
            };

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            imdbFileService.AddImdbFile(movieData);
        }
    }

    private static string? GetNullableField(string? value) =>
        string.IsNullOrWhiteSpace(value) || value == @"\N" ? null : value;
}
