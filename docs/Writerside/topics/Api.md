
# API

The API for %Product% can be accessed at `http://localhost:8181/scalar/v2` by default, where you can see more information on the endpoints outlined below.

Some endpoints can be disabled, so they do not get mapped by the Api, and then ultimately do not exist while %Product% is running.
Refer to [](Configuration.md) for more details on enabling/disabling endpoints.

## Authentication

Some Api Endpoints require authenticated requests. To perform these, include the `ApiKey` in the headers:
- **Header Key**: `X-Api-Key`
- **Header Value**: `<API_KEY>`

This `ApiKey` is generated on first run of %product%, and stored in the settings file: `%settings-file%` 

---

## Full List of Endpoints

### Blacklist Endpoints

> Blacklist endpoints allow you to add or remove torrents from the blacklist.
> Once a torrent is blacklisted, it will not be processed by %Product%.
> Adding a torrent to the blacklist will also remove it from the database.
{style="note"}


| Path                | Description                          | Authenticated |
|---------------------|--------------------------------------|---------------|
| `/blacklist/add`    | Add a torrent to the blacklist.      | Yes           |
| `/blacklist/remove` | Remove a torrent from the blacklist. | Yes           |


### Dmm Endpoints

> Dmm endpoints allow you to search for content, filter results, and ingest content.
> The Dmm import reruns on missing pages in the configured time interval see [](Configuration.md) but can be run on demand.
{style="note"}

| Path                    | Description               | Authenticated |
|-------------------------|---------------------------|---------------|
| `/dmm/search`           | Search for content.       | No            |
| `/dmm/filtered`         | Filter search results.    | No            |
| `/dmm/on-demand-scrape` | Ingest content on demand. | Yes           |

### Imdb Endpoints

> Imdb endpoints allow you to search for content via Imdb `tt` ids.
> The Imdb import of metadata occurs once on first run, and then every `14` days.
{style="note"}

| Path                | Description                          | Authenticated |
|---------------------|--------------------------------------|---------------|
| `/imdb/search`      | Search for content.                  | No            |

### Torznab Endpoints

> Torznab endpoints allow you to search for content via requests adhearing to the [Torznab Specification](https://torznab.github.io/spec-1.3-draft/torznab/Specification-v1.3.html#:~:text=Torznab%20is%20an%20api%20specification%20based%20on%20the,goal%20to%20supply%20an%20consistent%20api%20for%20torrents)
> Torznab is an api specification based on the Newznab WebAPI. The api is built around a simple xml/rss feed with filtering and paging capabilities. 
> The Torznab standard strives to be completely compliant with Newznab, insofar it does not conflict with its primary goal to supply an consistent api for torrents.
{style="note"}

| Path           | Description                            | Authenticated |
|----------------|----------------------------------------|---------------|
| `/torznab/api` | Search feeds using torznab parameters. | No            |

### Torrents Endpoints

> The Torrents endpoint exists so that an external system can Stream all the results in your database to their database.
> This endpoint is disabled by default, and can be enabled in the settings file see [](Configuration.md).
{style="note"}

| Path                | Description                          | Authenticated |
|---------------------|--------------------------------------|---------------|
| `/torrents/all`     | Stream all torrents in the database. | No            |

### Healthcheck Endpoints

> Healthcheck endpoints allow you to check the health of the %Product% service.
> The healthcheck endpoint is always available, and does not require authentication.

| Path                 | Description                          | Authenticated |
|----------------------|--------------------------------------|---------------|
| `/healthchecks/ping` | Check the health of the service.     | No            |