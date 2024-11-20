# Torznab Indexer: A Detailed Explanation

## What is a Torznab Indexer?

A **Torznab indexer** is a service or API that provides a standardized way to interact with torrent or Usenet indexers. It is part of the **"nzb" and "torrent" search ecosystem**, inspired by the NZB (Usenet) model but adapted for torrents. Torznab simplifies how applications like **Sonarr**, **Radarr**, or **Prowlarr** communicate with indexers by using a consistent interface.

The **Torznab protocol** is based on the RSS feed format and extends it with additional query parameters to enable searching, filtering, and retrieving torrent metadata efficiently.

---

## Purpose of Torznab Indexers

Torznab indexers serve as a bridge between **media management applications** (like Sonarr or Radarr) and the actual torrent trackers or Usenet servers. Their key purposes include:

1. **Centralized Management**:
    - Allow users to aggregate multiple torrent or Usenet indexers into a single application.
    - Enable media management tools to work seamlessly with various trackers.

2. **Standardization**:
    - Provide a uniform API for interacting with different indexers, which might otherwise have diverse and incompatible interfaces.

3. **Search and Discovery**:
    - Enable applications to perform automated searches for specific content (e.g., movies, TV shows, or software) using criteria such as keywords, categories, or file sizes.

4. **Automation**:
    - Facilitate hands-free searching and downloading of media based on predefined filters and schedules in applications like Radarr and Sonarr.

---

## How Does a Torznab Indexer Work?

Torznab indexers work by exposing a RESTful API that supports queries and responses in a consistent format. Here's how the workflow typically looks:

### 1. **Setup**
- Users configure the Torznab indexer URL and API key in their media application.
- Applications send requests to the indexer using the Torznab API.

### 2. **Request**
- The application makes a query to the Torznab indexer, specifying parameters such as:
    - **Search term**: Keywords for content (e.g., a movie name).
    - **Category**: Filters like movies, TV shows, or games.
    - **Limits**: File size, age, etc.

Example request: `GET /api?t=search&q=example_movie&cat=5000`

### 3. **Response**
- The Torznab indexer responds with an XML feed (based on RSS) that contains metadata about the search results.
- The response includes fields like:
    - Title
    - Link (usually a magnet link or torrent file URL)
    - Description
    - Category
    - Size

Example response (simplified):
```xml
```
{ src="example-torznab.xml" }

---

## Setting up as Torznab Indexer for Prowlarr

### Prowlarr

* Open Prowlarr, and navigate to `Indexers -> Add`
* Search for `generic`, with type `private`
* Add `Generic Torznab`
* Give it a name at the top (Zilean)
* Ensure `Url` is `http://zilean:8181/torznab`
* Ensure `API` box is `/api`
* Sync to Apps

Then move on to Radarr if using it

### Radarr

* Navigate to `/settings/indexers`
* On `Zilean` click edit
* Tick the Box `Remove year from search string`
* Save
