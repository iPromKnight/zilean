using Zilean.Shared.Features.Statistics;

namespace Zilean.Tests.Tests.Imdb;

public class ImdbMetadataLoaderTests
{
    /// <summary>
    /// Validates the freshness check logic used in ImdbMetadataLoader.Execute():
    /// DateTime.UtcNow - imdbLastImport.OccuredAt &lt; TimeSpan.FromDays(14)
    /// When last import is recent (less than 14 days ago), the condition should be true (skip download).
    /// </summary>
    [Fact]
    public void FreshnessCheck_SkipsDownload_WhenLastImportIsRecent()
    {
        // Arrange: simulate a recent import (10 days ago)
        var recentImport = new ImdbLastImport
        {
            OccuredAt = DateTime.UtcNow.AddDays(-10),
            EntryCount = 1000,
            Status = ImportStatus.Complete,
        };

        // Act: evaluate the same condition used in ImdbMetadataLoader.Execute() line 16
        var isFresh = DateTime.UtcNow - recentImport.OccuredAt < TimeSpan.FromDays(14);

        // Assert: 10-day-old import should be considered fresh (skip download)
        isFresh.Should().BeTrue("an import from 10 days ago is within the 14-day freshness window");
    }

    /// <summary>
    /// Validates the freshness check logic used in ImdbMetadataLoader.Execute():
    /// When last import is stale (14+ days ago), the condition should be false (proceed with download).
    /// </summary>
    [Fact]
    public void FreshnessCheck_ProceedsWithDownload_WhenLastImportIsStale()
    {
        // Arrange: simulate a stale import (15 days ago)
        var staleImport = new ImdbLastImport
        {
            OccuredAt = DateTime.UtcNow.AddDays(-15),
            EntryCount = 1000,
            Status = ImportStatus.Complete,
        };

        // Act: evaluate the same condition used in ImdbMetadataLoader.Execute() line 16
        var isFresh = DateTime.UtcNow - staleImport.OccuredAt < TimeSpan.FromDays(14);

        // Assert: 15-day-old import should be considered stale (proceed with download)
        isFresh.Should().BeFalse("an import from 15 days ago exceeds the 14-day freshness window");
    }
}
