# Zilean

DMM sourced arr-less searching for [Riven](https://github.com/rivenmedia/riven)

## What is Zilean?
Zilean is a service that allows you to search for [DebridMediaManager](https://github.com/debridmediamanager/debrid-media-manager) sourced arr-less content.
When the service is started, it will automatically download and index all the DMM shared hashlists and index them using Lucene.
The service provides a search endpoint that allows you to search for content using a query string, and returns a list of filenames and infohashes.
There is no clean filtering applied to the search results, the idea behind this endpoint is Riven performs that using [RTN](https://pypi.org/project/rank-torrent-name/).
The DMM import reruns on missing pages every hour.

## Configuration

```json
{
  "Zilean": {
    "Dmm": {
      "EnableScraping": true,
      "EnableEndpoint": true,
      "ScrapeSchedule": "0 * * * *",
      "MaxFilteredResults": 200,
      "MinimumScoreMatch": 0.85,
      "ImportBatched": false
    },
    "Database": {
      "ConnectionString": "Host=localhost;Database=zilean;Username=postgres;Password=postgres;Include Error Detail=true;Timeout=300;CommandTimeout=300;"
    },
    "Prowlarr": {
      "EnableEndpoint": true
    },
    "Imdb": {
      "EnableImportMatching": false,
      "EnableEndpoint": true,
      "MinimumScoreMatch": 0.85
    }
  }
}
```

Every option you see can be set as an env variable, the env variable name is the same as the json path with double underscores instead of dots.
For example, `Zilean__Dmm__EnableScraping` would be the env variable for `Zilean.Dmm.EnableScraping`.

A breakdown of all configuration options:

- `Zilean__Dmm__EnableScraping`: Whether to enable the DMM scraping service.
- `Zilean__Dmm__EnableEndpoint`: Whether to enable the DMM search endpoint.
- `Zilean__Dmm__ScrapeSchedule`: The cron schedule for the DMM scraping service.
- `Zilean__Dmm__MaxFilteredResults`: The maximum number of results to return from the DMM search endpoint.
- `Zilean__Dmm__MinimumScoreMatch`: The minimum score required for a search result to be returned. Values between 0 and 1. Defaults to 0.85.
- `Zilean__Dmm__ImportBatched`: Whether to import DMM pages in batches. This is for low end systems. Defaults to false. Will make the initial import take longer. A lot longer.
- `Zilean__Database__ConnectionString`: The connection string for the database (Postgres).
- `Zilean__Prowlarr__EnableEndpoint`: Whether to enable the Prowlarr search endpoint. (Unused currently).
- `Zilean__Imdb__EnableImportMatching`: Whether to enable the IMDB import matching service. Defaults to true. Disabling this will improve import speed at the cost of not having IMDB data.
- `Zilean__Imdb__EnableEndpoint`: Whether to enable the IMDB search endpoint.
- `Zilean__Imdb__MinimumScoreMatch`: The minimum score required for a search result to be returned. Values between 0 and 1. Defaults to 0.85.
---

## Compose Example
See the file [compose.yaml](https://github.com/iPromKnight/zilean/blob/main/compose.yaml) for an example of how to run Zilean.