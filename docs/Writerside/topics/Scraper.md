# Scraper

The zilean scraper is the service responsible for ingestion of all content.
This also includes metadata from imdb.

While the scraper does run as a scheduled task, it can also be run on demand, and manually from within the container.
The scraper exists at `/app/scraper`

## Running the Scraper

> Note: When doing this - ensure you have stopped Your api instance, as the scraper and the api scheduler scraping
> tasks will cause data corruption.
{style="warning"}

To run the scraper manually, you can use the following command:

```bash
docker compose stop zilean

docker run -it --rm \
    -v ./settings.json:/app/data/settings.json \
    --name zileanscraper \
    ipromknight/zilean:latest \
    /app/scraper <args>
```

This will run the scraper, and output the results to the console.

The following arguments are available when running the scraper:

| Argument            | Description                                                                                                |
|---------------------|------------------------------------------------------------------------------------------------------------|
| `dmm-sync`          | Run the DMM scraper to import DMM Hash Lists.                                                              |
| `generic-sync`      | Run the Ingestion syncer to sync from Zilean and Zurg instances in your config.                            |
| `resync-imdb -s`    | Force re-ingest IMDB metadata.                                                                             |
| `resync-imdb -s -t` | Force re-ingest IMDB metadata, then try to update any entries in your database that do not have an imdb id |
| `resync-imdb -t`    | Try to update any entries in your database that do not have an imdb id                                     |
| `resync-imdb -s -a` | Force re-ingest IMDB metadata, then Rematch / update any entries in your database with a new ImdbID        |
| `resync-imdb -a`    | Rematch / update any entries in your database with a new ImdbID                                            |

## Examples

### Running the IMDB Resync with Title Update

```bash
docker exec -it zilean \
    /app/scraper resync-imdb -s -t
```

### Running the IMDB Resync to rematch ALL Titles

```bash
docker exec -it zilean \
    /app/scraper resync-imdb -a
```

### Running the DMM Sync

```bash
docker exec -it zilean \
    /app/scraper dmm-sync
```

### Running the Generic Sync

```bash
docker exec -it zilean \
    /app/scraper generic-sync
```
