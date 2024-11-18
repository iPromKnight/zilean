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
    "ApiKey": "69f72d7eb22e48938fd889206ffcf911a514bcc2e3824b2e9e7549122fb16849",
    "FirstRun": true,
    "Dmm": {
      "EnableScraping": true,
      "EnableEndpoint": true,
      "ScrapeSchedule": "0 * * * *",
      "MinimumReDownloadIntervalMinutes": 30,
      "MaxFilteredResults": 200,
      "MinimumScoreMatch": 0.85
    },
    "Torznab": {
      "EnableEndpoint": true
    },
    "Database": {
      "ConnectionString": "Host=localhost;Database=zilean;Username=postgres;Password=postgres;Include Error Detail=true;Timeout=30;CommandTimeout=3600;"
    },
    "Torrents": {
      "EnableEndpoint": false
    },
    "Imdb": {
      "EnableImportMatching": true,
      "EnableEndpoint": true,
      "MinimumScoreMatch": 0.85
    },
    "Ingestion": {
      "ZurgInstances": [],
      "ZileanInstances": [],
      "EnableScraping": false,
      "Kubernetes": {
        "EnableServiceDiscovery": false,
        "KubernetesSelectors": [],
        "KubeConfigFile": "/$HOME/.kube/config",
        "AuthenticationType": 0
      },
      "ScrapeSchedule": "0 * * * *",
      "ZurgEndpointSuffix": "/debug/torrents",
      "ZileanEndpointSuffix": "/torrents/all",
      "RequestTimeout": 10000
    },
    "Parsing": {
      "IncludeAdult": false,
      "IncludeTrash": true,
      "BatchSize": 5000
    }
  }
}
```
### This file can be edited on your disk and mounted as a volume into the container at `/app/data/settings.json`.

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
- `Zilean__Torznab__EnableEndpoint`: Whether to enable the Torznab endpoints.
---

## Compose Example
See the file [compose.yaml](https://github.com/iPromKnight/zilean/blob/main/compose.yaml) for an example of how to run Zilean.

---

## API

The Api can be accessed at `http://localhost:8181/scalar/v2` by default, which allows you to execute
any of the available endpoints directly in a [Scalar](https://github.com/ScalaR/ScalaR) dashboard.

---


# Generic Ingestion Setup

Zilean now has new **Generic Ingestion** functionality added. This setup provides a flexible mechanism to discover and process torrent data from multiple endpoints, including Kubernetes services and direct zurg and other zilean endpoint URLs.

---

## Ingestion Configuration

The `Ingestion` section in the JSON configuration defines the behavior and options for the generic ingestion process.

### Example Configuration

```json
"Ingestion": {
  "ZurgInstances": [],
  "ZileanInstances": [],
  "EnableScraping": false,
  "Kubernetes": {
    "EnableServiceDiscovery": false,
    "KubernetesSelectors": [
      {
        "UrlTemplate": "http://zurg.{0}:9999",
        "LabelSelector": "app.elfhosted.com/name=zurg",
        "EndpointType": 1
      }
    ],
    "KubeConfigFile": "/$HOME/.kube/config",
    "AuthenticationType": 0
  },
  "ScrapeSchedule": "0 * * * *",
  "ZurgEndpointSuffix": "/debug/torrents",
  "ZileanEndpointSuffix": "/torrents/all",
  "RequestTimeout": 10000
}
```

---

## Key Fields

### `ZurgInstances`
- **Type**: `GenericEndpoint[]`
- **Description**: A list of direct endpoints for Zurg instances.
- **Structure**:
  ```json
  {
    "Url": "http://zurg.example.com:19999",
    "EndpointType": 1
  }
  ```
- **Example**:
  ```json
  "ZurgInstances": [
    {
      "Url": "http://zurg.prod.cluster.local:19999",
      "EndpointType": 1
    }
  ]
  ```

### `ZileanInstances`
- **Type**: `GenericEndpoint[]`
- **Description**: A list of direct endpoints for Zilean instances.
- **Structure**:
  ```json
  {
    "Url": "http://zilean.example.com:8181",
    "EndpointType": 0
  }
  ```
- **Example**:
  ```json
  "ZileanInstances": [
    {
      "Url": "http://zilean.prod.cluster.local:8181",
      "EndpointType": 0
    }
  ]
  ```

### `EnableScraping`
- **Type**: `bool`
- **Description**: Enables or disables automated scraping functionality for ingestion.

### `Kubernetes`
- **Type**: `object`
- **Description**: Configuration for Kubernetes-based service discovery.
- **Fields**:
    - **`EnableServiceDiscovery`**: Enables Kubernetes service discovery.
    - **`KubernetesSelectors`**:
        - **`UrlTemplate`**: Template for constructing URLs from Kubernetes service metadata.
        - **`LabelSelector`**: Label selector to filter Kubernetes services.
        - **`EndpointType`**: Indicates the type of endpoint (0 = Zilean, 1 = Zurg).
    - **`KubeConfigFile`**: Path to the Kubernetes configuration file.
    - **`AuthenticationType`**: Authentication type for Kubernetes service discovery (0 = ConfigFile, 1 = RoleBased).

### `ScrapeSchedule`
- **Type**: `string` (CRON format)
- **Description**: Schedule for automated scraping tasks.

### `ZurgEndpointSuffix`
- **Type**: `string`
- **Description**: Default suffix appended to Zurg instance URLs for ingestion.

### `ZileanEndpointSuffix`
- **Type**: `string`
- **Description**: Default suffix appended to Zilean instance URLs for ingestion.

### `RequestTimeout`
- **Type**: `int`
- **Description**: Timeout for HTTP requests in milliseconds.

---

## `GenericEndpoint` and `GenericEndpointType`

### `GenericEndpoint`
Represents a generic endpoint configuration.

```csharp
public class GenericEndpoint
{
    public required string Url { get; set; }
    public required GenericEndpointType EndpointType { get; set; }
}
```

### `GenericEndpointType`
Defines the type of an endpoint.

```csharp
public enum GenericEndpointType
{
    Zilean = 0,
    Zurg = 1
}
```

---

## New Torrents Configuration

### Example

```json
"Torrents": {
  "EnableEndpoint": false
}
```

- **`EnableEndpoint`**:
    - **Type**: `bool`
    - **Description**: Enables or disables the Torrents API endpoint `/torrents/all` which allows this zilean instance to be scraped by another.

---

## Kubernetes Service Discovery

If `EnableServiceDiscovery` is set to `true` in the Kubernetes section, the application will use the Kubernetes API to discover services matching the provided `LabelSelector`. The discovered services will be processed using the specified `UrlTemplate` and their `EndpointType`.

### Example Service Discovery Configuration

```json
"Kubernetes": {
  "EnableServiceDiscovery": true,
  "KubernetesSelectors": [
    {
      "UrlTemplate": "http://zurg.{0}:9999",
      "LabelSelector": "app.elfhosted.com/name=zurg",
      "EndpointType": 1
    }
  ],
  "KubeConfigFile": "/$HOME/.kube/config",
  "AuthenticationType": 0
}
```
### `AuthenticationType`
Defines the Types of authentication to use when connecting to the kubernetes service host.

```csharp
public enum KubernetesAuthenticationType
{
    ConfigFile = 0,
    RoleBased = 1
}
```
note: In order for RBAC to work, the service account must have the correct permissions to list services in the namespace, and the `KUBERNETES_SERVICE_HOST` and `KUBERNETES_SERVICE_PORT` environment variables must be set.

### Behavior
1. The application uses the Kubernetes client to list services matching the `LabelSelector`.
2. It generates service URLs using the `UrlTemplate`, where `{0}` is replaced by the namespace.
3. These URLs are passed to the ingestion pipeline for processing.

---

## Integration with Ingestion Pipeline

The ingestion pipeline combines direct endpoints (`ZurgInstances` and `ZileanInstances`) and Kubernetes-discovered services (if enabled) to create a unified list of URLs. These URLs are then processed in batches, filtering out torrents already stored in the database.

---

## Logging and Monitoring

Key events in the ingestion process are logged:
- Discovered URLs.
- Filtered torrents (existing in the database).
- Processed torrents (new and valid).
- Errors during processing or service discovery.

---

## Blacklisting

The ingestion pipeline supports blacklisting infohashes to prevent them from being processed. This feature is useful for filtering out unwanted torrents or duplicates.
See the `/blacklist` endpoints for more information in scalar.
These endpoints are protected by the ApiKey that will be generated on first run of the application and stored in the settings.json file as well as a one time print to application logs on startup.
Blacklisting an item also removes it from the database.

---

## Api Key

The ApiKey is generated on first run of the application and stored in the settings.json file as well as a one time print to application logs on startup.
The key can also be cycled to a new key if you set the environment variable `ZILEAN__NEW__API__KEY` to `true` and restart the application.
To authenticate with the API, you must include the `ApiKey` in the request headers. The header key is `X-Api-Key` and will automatically be configured in scalar.