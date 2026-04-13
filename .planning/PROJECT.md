# Zilean

## What This Is

Zilean is a .NET 9 torrent indexing service that scrapes DMM hashlists and other sources, enriches torrent metadata with IMDb matching, and exposes a Torznab-compatible API for media management tools like Sonarr and Radarr. It's used by self-hosters running Debrid-based media setups.

## Core Value

Torrents are correctly identified and searchable — a search for any media title (in any language) returns the right results with accurate IMDb metadata.

## Requirements

### Validated

- ✓ Scrape and ingest torrent metadata from DMM, Zurg, and peer Zilean instances — existing
- ✓ Parse torrent names into structured metadata (title, year, season, episode) via rank-torrent-name — existing
- ✓ Match torrents to IMDb IDs using Lucene fuzzy search — existing
- ✓ Full-text search across torrent database — existing
- ✓ Torznab API for Sonarr/Radarr integration — existing
- ✓ API key authentication for protected endpoints — existing
- ✓ Background job scheduling for periodic sync (DMM, Generic) — existing
- ✓ Kubernetes service discovery for auto-ingestion — existing
- ✓ Multi-arch Docker deployment (amd64, arm64) — existing
- ✓ OpenTelemetry observability — existing

### Active

- [ ] Audit and improve code quality, fix bugs, reduce technical debt
- [ ] Improve test coverage and add CI quality gates
- [ ] Match torrents using IMDb alternative/original titles (title.akas.tsv) for non-English media
- [ ] Streamline local development and build process

### Out of Scope

- Mobile app — Zilean is a backend service, not user-facing
- Real-time push notifications — Zilean is pull-based by design
- Multi-database support — PostgreSQL is the only supported backend
- Web UI overhaul — existing Blazor dashboard is sufficient

## Context

- Brownfield project with active community contributions (GitHub PRs)
- Current IMDb matching only uses `title.basics.tsv` (primaryTitle field) — torrents with non-English titles (e.g. French film "Un p'tit truc en plus") fail to match because only the English title is indexed
- IMDb provides `title.akas.tsv` with localized/original titles that could solve this
- The Lucene index in `ImdbLuceneMatchingService` indexes one document per IMDb entry with only the primary title
- Test suite exists (xUnit + Testcontainers) but coverage is minimal (2 test files)
- CI/CD exists via GitHub Actions but only triggers on version tags, no PR checks
- Python interop (pythonnet) for torrent name parsing adds deployment complexity

## Constraints

- **Tech stack**: .NET 9, PostgreSQL, existing architecture must be preserved
- **Compatibility**: API contract (Torznab, /dmm/search, /all/checkcached) must remain backwards-compatible
- **Deployment**: Docker-based, multi-arch (amd64/arm64)
- **Dependencies**: IMDb public datasets as source of truth for metadata

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Use title.akas.tsv for original title matching | IMDb already provides this data, no new external dependency | — Pending |
| Index alternative titles in existing Lucene index | Reuses existing matching infrastructure, no new search engine needed | — Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-04-13 after initialization*
