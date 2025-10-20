# Getting Started

<img src="zilean-logo.jpg" alt="Zilean Logo" width="300" height="300" />

## What is %Product%?

%Product% is a service that allows you to search for [DebridMediaManager](https://github.com/debridmediamanager/debrid-media-manager) sourced content shared by users.
The DMM import reruns on missing pages in the configured time interval see [](Configuration.md).

This can then be configured as a Torznab indexer in your favorite content application.

Newly added is the ability for %Product% to scrape from your running Zurg instance, and from other running %Product% instances.

## Installation

The easiest way to get up and running with %Product% is to use the provided docker-compose file.

Ensure you have the following installed:
- Docker, Docker Desktop or Podman

The example compose file below can be copied, and used to get the system running locally.

```yaml
```
{ src="compose-file.yaml" }

This compose file will start the following services:
- %Product%
- Postgres (version 17)

The configuration and persistent storage of both services will be stored in docker volumes, but i recommend changing this if you are not on windows to the `./data` directory, next to where the compose file resides.

## Pulling Latest Image

If you would like to pull the latest image from the docker registry, you can use the following command:

```bash
docker compose pull %product%
```

> Please Note - Always make sure you check the [github release notes](https://github.com/iPromKnight/zilean/releases) for the latest release, to ensure there are no breaking changes.
> The changelog can also be viewed [here](https://github.com/iPromKnight/zilean/blob/main/CHANGELOG.md).
{ style="note" }
