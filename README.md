# Zilean

DMM sourced arr-less searching for [Riven](https://github.com/rivenmedia/riven)

## What is Zilean?
Zilean is a service that allows you to search for [DebridMediaManager](https://github.com/debridmediamanager/debrid-media-manager) sourced arr-less content.
When the service is started, it will automatically download and index all the DMM shared hashlists and index them using Lucene.
The service provides a search endpoint that allows you to search for content using a query string, and returns a list of filenames and infohashes.
There is no clean filtering applied to the search results, the idea behind this endpoint is Riven performs that using [RTN](https://pypi.org/project/rank-torrent-name/).
The DMM import reruns on missing pages every hour.

## Configuration

There is no configuration for Zilean.
For persistence, you can mount a volume on the path `/app/data`.
---

## Compose
```yaml
volumes:
  zilean_data:

services:
  zilean:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8181:8181"
    volumes:
      - zilean_data:/app/data
````
---

## Endpoints

### Search Endpoint

Endpoint
```bash
POST /dmm/search
```

Request Body
```json
{
    "queryText": "string"
}
```

### Responses
Success:
```json
[
  {
    "filename": "string",
    "infoHash": "string",
    "filesize": "long"
  },
  {
    "filename": "string",
    "infoHash": "string",
    "filesize": "long"
  }
]
```

Error:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "string",
  "status": 500,
  "detail": "string"
}
```