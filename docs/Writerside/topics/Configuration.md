
# Configuration

Configuration for the Zilean application is defined in a JSON file, which can be mounted as a volume into the container at `/app/data/settings.json`.

> The LOAD ORDER of the configuration is as follows:
> Settings are loaded first from `/app/data/settings.json`, then from environment variables, as these take precedence.
{ style="note" }

The format of environment variables is as follows:
`Zilean__{Section}__{Key}`
Where `{Section}` is the section of the configuration, and `{Key}` is the key within that section.

For example, to set the `ApiKey` in the `Zilean` section, you would set the environment variable `Zilean__ApiKey` or
`Zilean__Dmm__EnableScraping` for the `Dmm.EnableScraping` key.

### Example Configuration

```json
```
{ src="default-settings.json" }

### Configuration Options in Detail

**ApiKey**
_The API key used to authenticate requests to the Zilean API._
_Default: `Generated on first run`_

**FirstRun**
_Indicates whether this is the first run of the application._
_Default: `true`_

### DMM Configuration
**Dmm.EnableScraping**
_Indicates whether the DMM indexer should scrape from Dmm Hashlists._
_Default: `true`_

**Dmm.EnableEndpoint**
_Indicates whether the DMM indexer should expose an API endpoint._
_Default: `true`_

**Dmm.ScrapeSchedule**
_The cron schedule for the DMM indexer to scrape from Dmm Hashlists._
_Default: `0 * * * *` [Hourly]_

**Dmm.MinimumReDownloadIntervalMinutes**
_The minimum interval in minutes before re-downloading Dmm Hashlists._
_Default: `30`_

**Dmm.MaxFilteredResults**
_The maximum number of filtered results to return from the DMM Search Endpoints._
_Default: `200`_

**Dmm.MinimumScoreMatch**
_The minimum score match for DMM search results. Closer to 1 means a more exact match has to occur. A value between 1 and 0._
_Default: `0.85`_

### Torznab Configuration
**Torznab.EnableEndpoint**
_Indicates whether the Torznab indexer should expose an API endpoint._
_Default: `true`_

### Database Configuration
**Database.ConnectionString**
_The connection string for the PostgreSQL database._
_Default: `Host=localhost;Database=zilean;Username=postgres;Password=postgres;Include Error Detail=true;Timeout=30;CommandTimeout=3600;`_

The database connection string comprises of the following:
- `Host`: The host of the database, this will usually be the `containername` if you are using docker compose of the postgres instance.
- `Database`: The name of the database to connect to.
- `Username`: The username to connect to the database with.
- `Password`: The password to connect to the database with.
- `Include Error Detail`: Whether to include error details in logging database results.
- `Timeout`: The timeout in seconds for the database connection.
- `CommandTimeout`: The timeout in seconds for database commands to occur, such as applying migrations.

### Torrents Configuration
**Torrents.EnableEndpoint**
_Indicates whether the Torrents API allowing other apps to scrape the Zilean database is enabled._
_Default: `false`_

### IMDB Configuration
**Imdb.EnableImportMatching**
_Indicates whether the indexer should import match titles to IMDB Ids during importing._
_Default: `true`_

**Imdb.EnableEndpoint**
_Indicates whether the IMDB indexer should expose an API endpoint._
_Default: `true`_

**Imdb.MinimumScoreMatch**
_The minimum score match for IMDB search results. Closer to 1 means a more exact match has to occur. A value between 1 and 0._
_Default: `0.85`_

### Ingestion Configuration
**Ingestion.ZurgInstances**
_A list of Zurg instances to scrape from._
_Default: `[]`_

**Ingestion.ZileanInstances**
_A list of Zilean instances to scrape from._
_Default: `[]`_

**Ingestion.EnableScraping**
_Indicates whether the Ingestion indexer should scrape from Zurg and Zilean instances._
_Default: `false`_

#### Kubernetes Configuration for Ingestion
**Ingestion.Kubernetes.EnableServiceDiscovery**
_Indicates whether the Ingestion indexer should use Kubernetes service discovery. This can be used to automatically find Zurg instances running in Kubernetes._
_Default: `false`_

**Ingestion.Kubernetes.KubernetesSelectors**
_A list of selectors to use for Kubernetes service discovery._
_Default: `[]`_

**Ingestion.Kubernetes.KubeConfigFile**
_The path to the Kubernetes configuration file._
_Default: `/$HOME/.kube/config`_

**Ingestion.Kubernetes.AuthenticationType**
_The type of authentication to use for Kubernetes service discovery. 0 = None, 1 = Kubernetes RBAC._
_Default: `0`_

**Ingestion.ScrapeSchedule**
_The cron schedule for the Ingestion indexer to scrape from Zurg and Zilean instances._
_Default: `0 0 * * *` [Daily]_

**Ingestion.ZurgEndpointSuffix**
_The endpoint suffix for the Zurg API._
_Default: `/debug/torrents`_

**Ingestion.ZileanEndpointSuffix**
_The endpoint suffix for the Zilean API._
_Default: `/torrents/all`_

**Ingestion.RequestTimeout**
_The timeout in milliseconds for requests to Zurg and Zilean instances._
_Default: `10000`_

### Parsing Configuration
**Parsing.IncludeAdult**
_Indicates whether adult content should be included in the indexer._
_Default: `false`_

**Parsing.IncludeTrash**
_Indicates whether trash content should be included in the indexer._
_Default: `true`_

**Parsing.BatchSize**
_The batch size for parsing content._
_Default: `5000`_

## Enabling Ingestion

To enable ingestion, set the `Ingestion.EnableScraping` key to `true` in the configuration.
Also ensure that the `Ingestion.ZurgInstances` and or `Ingestion.ZileanInstances` keys are populated with the appropriate `Url`, and `EndpointType` values.
`EndpointType` can be either `0` (Zurg) or `1` (Zilean).
You do not have to specify both, you can specify one or the other, or both, depending on your requirements.
Also there is no limit to the number of instances you can scrape from.

An example of this configuration is as follows:

```json
```
{ src="settings-with-ingestion.json" }