<!-- GSD:project-start source:PROJECT.md -->
## Project

**Zilean**

Zilean is a .NET 9 torrent indexing service that scrapes DMM hashlists and other sources, enriches torrent metadata with IMDb matching, and exposes a Torznab-compatible API for media management tools like Sonarr and Radarr. It's used by self-hosters running Debrid-based media setups.

**Core Value:** Torrents are correctly identified and searchable — a search for any media title (in any language) returns the right results with accurate IMDb metadata.

### Constraints

- **Tech stack**: .NET 9, PostgreSQL, existing architecture must be preserved
- **Compatibility**: API contract (Torznab, /dmm/search, /all/checkcached) must remain backwards-compatible
- **Deployment**: Docker-based, multi-arch (amd64/arm64)
- **Dependencies**: IMDb public datasets as source of truth for metadata
<!-- GSD:project-end -->

<!-- GSD:stack-start source:codebase/STACK.md -->
## Technology Stack

## Languages
- C# 12 (with latest lang version enabled) - All application code, tests, utilities
- Python 3.11 - Torrent title parsing via `rank-torrent-name` library, integrated via pythonnet
## Runtime
- .NET 9.0 (net9.0 target framework)
- Alpine Linux 3.18 (container runtime)
- Python 3.11 (embedded in container via apk)
- NuGet 6.x (implicit with .NET 9.0 SDK)
- Lockfile: `Directory.Packages.props` (central version management)
- Python pip for Python dependencies in `requirements.txt`
## Frameworks
- ASP.NET Core 9.0 - Web API framework for `Zilean.ApiService`
- Entity Framework Core 9.0 - ORM for data access in `Zilean.Database`
- .NET Generic Host - Host builder for `Zilean.Scraper` CLI application
- Coravel 6.0.0 - Task scheduling and invocable jobs
- Spectre.Console.Cli 0.49.2-preview.0.50 - CLI command framework for scraper
- xUnit 2.9.2 - Test framework
- NSubstitute 5.3.0 - Mocking framework
- Testcontainers 4.0.0 with Testcontainers.PostgreSql 4.0.0 - Container-based test fixtures
- FluentAssertions 6.12.2 - Assertion library
- Verify.Xunit 28.3.2 - Snapshot/approval testing
- BenchmarkDotNet 0.14.0 - Performance benchmarking
## Key Dependencies
- Npgsql.EntityFrameworkCore.PostgreSQL 9.0.0 - PostgreSQL EF Core provider, handles all database operations
- Npgsql (implicit via EF Core) - PostgreSQL ADO.NET data provider
- Dapper 2.1.35 - Lightweight ORM for complex SQL queries
- EFCore.BulkExtensions.PostgreSql 8.1.1 - Bulk insert/update operations for torrent data ingestion
- pythonnet 3.0.4 - .NET/Python interop, enables calling Python libraries from C#
- Lucene.Net 4.8.0-beta00017 - Full-text search indexing engine (core search functionality)
- Lucene.Net.Analysis.Common 4.8.0-beta00017 - Text analysis for search
- Lucene.Net.QueryParser 4.8.0-beta00017 - Query parsing for Lucene
- Raffinert.FuzzySharp 2.0.3 - Fuzzy string matching for torrent search
- Serilog.Sinks.Spectre 0.5.0 - Structured logging with console output
- Spectre.Console 0.49.2-preview.0.50 - Rich console UI and formatting
- CliWrap 3.6.7 - Shell command execution wrapper for running scraper commands
- CsvHelper 33.0.1 - CSV parsing for data imports
- Microsoft.IO.RecyclableMemoryStream 3.0.1 - Memory pooling for high-throughput operations
- Microsoft.AspNetCore.OpenApi 9.0.0 - OpenAPI/Swagger support
- Scalar.AspNetCore 1.2.39 - Interactive API documentation UI
- Syncfusion.Blazor.Grid 27.2.2 - Dashboard UI grid component
- Syncfusion.Blazor.Themes 27.2.2 - Dashboard themes
- SimCube.Aspire 1.0.5 - OTLP (OpenTelemetry) defaults
- Microsoft.Extensions.Http.Resilience 9.0.0 - HTTP client resilience patterns (retries, timeouts, circuit breakers)
- System.Linq.Async 6.0.1 - Async LINQ support
- KubernetesClient 15.0.1 - Kubernetes API client for service discovery
- TestableIO.System.IO.Abstractions 21.1.3 - Abstraction layer for file I/O testing
- TestableIO.System.IO.Abstractions.TestingHelpers 21.1.3 - Testing helpers for file I/O
## Configuration
- JSON configuration files in `config/` directory (created if missing)
- Configuration file: `settings.json` at runtime
- Environment variables override configuration values
- `.env` file support (not committed to git, contains secrets)
- `Directory.Build.props` - Global build properties (target framework, documentation, nullable reference types)
- `Directory.Build.targets` - Global build targets
- `Directory.Packages.props` - Central package version management
- `nuget.config` - NuGet package sources configuration
- `ZileanConfiguration` class manages all settings
- Sections: Dmm, Torznab, Database, Torrents, Imdb, Ingestion, Parsing
## Platform Requirements
- .NET 9.0 SDK
- PostgreSQL 17.1+ (via Docker)
- Python 3.11 (for local development)
- Docker / Docker Compose (for local development)
- Container-based deployment via `Dockerfile`
- Alpine Linux 3.18 base image
- PostgreSQL 17.1+ (external database)
- Python 3.11 (bundled in container)
- Minimum resources: Custom based on data volume
<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->
## Conventions

Conventions not yet established. Will populate as patterns emerge during development.
<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->
## Architecture

## Pattern Overview
- Feature-driven folder structure (`Features/*` in each project)
- Clean separation between API service (Zilean.ApiService), scraper (Zilean.Scraper), database (Zilean.Database), and shared utilities (Zilean.Shared)
- Dependency injection throughout for loose coupling and testability
- Background job scheduling using Coravel
- Bulk data operations for high-throughput ingestion
## Layers
- Purpose: Expose REST API endpoints for torrent search, cache checking, management operations
- Location: `src/Zilean.ApiService/`
- Contains: Endpoint handlers, authentication middleware, background job triggers
- Depends on: `Zilean.Database`, `Zilean.Shared`
- Used by: External clients (apps like Sonarr, Radarr), end users
- Purpose: Execute background jobs to fetch, parse, and store torrent metadata
- Location: `src/Zilean.Scraper/`
- Contains: CLI commands, endpoint processors, file downloaders, metadata processors
- Depends on: `Zilean.Database`, `Zilean.Shared`
- Used by: API service via scheduled jobs or on-demand triggers
- Purpose: Encapsulate all database operations via Entity Framework Core and Dapper
- Location: `src/Zilean.Database/`
- Contains: DbContext, model configurations, services (TorrentInfoService, ImdbMatchingService), SQL functions, migrations
- Depends on: `Zilean.Shared` (models)
- Used by: API service and Scraper
- Purpose: Define domain models, configurations, utilities, and extension methods
- Location: `src/Zilean.Shared/`
- Contains: Configuration classes, domain models (TorrentInfo, ImdbFile), Python interop, logging, utilities
- Depends on: System libraries only
- Used by: All other projects
- Purpose: PostgreSQL database with Lucene.Net full-text search indexing
- Technology: PostgreSQL 17.1+, Lucene.Net 4.8
- Operations: Bulk inserts/updates via EFCore.BulkExtensions, raw SQL queries via Dapper
- Indexes: Pre-computed search indexes via SQL functions like `search_torrents_meta()`
## Data Flow
- Transient: HTTP request state (current search query, API key)
- Scoped: DI container scope state (DbContext per request, transaction scope)
- Singleton: Application state (configuration, logging, scheduled job registry via Coravel)
- Background: Mutex-protected sync jobs to prevent concurrent executions
## Key Abstractions
- Purpose: Unified interface for torrent search and storage operations
- Implementations: `TorrentInfoService` in `src/Zilean.Database/Services/`
- Pattern: Service-oriented with dependency injection
- Key methods: `StoreTorrentInfo()`, `SearchForTorrentInfoFiltered()`, `SearchForTorrentInfoByOnlyTitle()`, `VaccumTorrentsIndexes()`
- Purpose: Domain-specific operations for DMM torrent records
- Location: `src/Zilean.Database/Services/DmmService.cs`
- Handles: DMM record parsing and validation
- Purpose: Match torrent metadata to IMDb entries
- Handles: Populate IMDb data cache, match batch torrents to IMDb IDs
- Key methods: `PopulateImdbData()`, `MatchImdbIdsForBatchAsync()`, `DisposeImdbData()`
- Purpose: IMDb file operations and queries
- Location: `src/Zilean.Database/Services/ImdbFileService.cs`
- Purpose: Wrapper around Python `rank-torrent-name` library
- Location: `src/Zilean.Shared/Features/Python/ParseTorrentNameService.cs`
- Pattern: Pythonnet interop, caches Python module in memory
- Returns: `ParseTorrentTitleResponse` with parsed title components
- Location: `src/Zilean.Scraper/Features/Ingestion/Processing/`
- Pattern: Separate classes for different data sources (DmmFileEntryProcessor, StreamedEntryProcessor, GenericProcessor)
- Responsibility: Transform external data format to `TorrentInfo` objects
- Framework: Coravel Invocable pattern
- Location: `src/Zilean.ApiService/Features/Sync/`
- Jobs: `DmmSyncJob`, `GenericSyncJob`
- Scheduling: Configured via `ZileanConfiguration` with cron expressions
## Entry Points
- Location: `src/Zilean.ApiService/Program.cs`
- Triggers: Application startup
- Responsibilities: 
- Location: `src/Zilean.Scraper/Program.cs`
- Triggers: Manual command invocation or scheduled job dispatch
- Responsibilities:
- Entry point: xUnit test discovery in `tests/Zilean.Tests/`
- Fixtures: `PostgresLifecycleFixture` manages test database containers via Testcontainers
- Pattern: Collections for shared fixtures across test classes
## Error Handling
- Try-catch in endpoint handlers with `ILogger<T>.LogError()`
- Validation errors returned as `BadRequest<ErrorResponse>` in API responses
- Database exceptions propagate with SQL context logged
- Async exceptions captured and logged in background jobs
- Python interop failures caught and logged with exception details
## Cross-Cutting Concerns
- Serilog structured logging configured in `LoggingConfiguration` 
- Spectre.Console sink for colorized console output
- Structured properties preserved in JSON log format
- Used throughout via `ILogger<T>` dependency injection
- API request validation via `BadRequest` results
- Configuration validation at startup
- Domain model validation via data annotations
- API key-based via `ApiKeyAuthentication` handler
- Custom `AuthenticationHandler` implementation
- Applied to sensitive endpoints via `RequireAuthorization()` attribute
- Metadata: `OpenApiSecurityMetadata` for OpenAPI documentation
- Centralized `ZileanConfiguration` class loaded from `settings.json`
- Sections: Dmm, Torznab, Database, Torrents, Imdb, Ingestion, Parsing
- Immutable at runtime (configuration changes require restart)
- Fallback defaults for missing values
- Mutex-based locking for sync jobs (prevent concurrent DMM/Generic syncs)
- `IMutex` service tracks lock state
- `SyncOnDemandState` service tracks job execution status
- `IServiceScope` for dependency injection scoping per operation
- `DbContext` pooling via Entity Framework Core
- Lucene index session lifecycle management
- Python module cached in singleton scope
<!-- GSD:architecture-end -->

<!-- GSD:skills-start source:skills/ -->
## Project Skills

No project skills found. Add skills to any of: `.claude/skills/`, `.agents/skills/`, `.cursor/skills/`, or `.github/skills/` with a `SKILL.md` index file.
<!-- GSD:skills-end -->

<!-- GSD:workflow-start source:GSD defaults -->
## GSD Workflow Enforcement

Before using Edit, Write, or other file-changing tools, start work through a GSD command so planning artifacts and execution context stay in sync.

Use these entry points:
- `/gsd-quick` for small fixes, doc updates, and ad-hoc tasks
- `/gsd-debug` for investigation and bug fixing
- `/gsd-execute-phase` for planned phase work

Do not make direct repo edits outside a GSD workflow unless the user explicitly asks to bypass it.
<!-- GSD:workflow-end -->



<!-- GSD:profile-start -->
## Developer Profile

> Profile not yet configured. Run `/gsd-profile-user` to generate your developer profile.
> This section is managed by `generate-claude-profile` -- do not edit manually.
<!-- GSD:profile-end -->
