# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

BldLeague is an ASP.NET Core 10 web application for managing a BLD (Blindfolded) speedcubing league. It tracks seasons, leagues, rounds, matches, and player standings. The UI is in Polish.

## Commands

### Build & Run
```bash
dotnet build BldLeague.slnx
dotnet run --project src/Web
docker compose up --build   # full stack with PostgreSQL
```

### Database Migrations
```bash
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/Web
```

### Configuration
- Connection string goes under `ConnectionStrings:Default` in `appsettings.json` or user secrets.
- WCA OAuth credentials (`ClientId`, `ClientSecret`) go under the `WCA` section — use user secrets in development (`UserSecretsId` is set in `Web.csproj`).
- Database migrations are applied automatically on startup via `EnsureMigratedHelper`.

## Development Workflow

### Issues
- Every piece of work starts with a GitHub issue.
- Use the existing issue templates (feature request, hotfix) where applicable.
- Every issue must have exactly one **type** label (`type: bug`, `type: chore`, `type: feature`) and one **priority** label (`priority: low`, `priority: medium`, `priority: high`).

### Branch Naming
Always create new branches from `main`.

Branches that reference an issue must include the issue number and follow this pattern:
- `feature/15-short-description` — new features (`type: feature`)
- `fix/12-short-description` — bug fixes (`type: bug`)
- `chore/7-short-description` — chores (`type: chore`)

Other branch types (`refactor/something`, `project-management/something`, etc.) are also valid but are not tied to any issue and will not trigger project automation workflows.

### Pull Requests & Merge Flow
- All PRs target `staging` first — never merge a feature/fix branch directly into `main`.
- PR body must include `Closes #<issue-number>` so the issue is linked and automation triggers correctly.
- Once staging is verified, `staging` is merged into `main`.

### Testing & Verification
- Do **not** put a test plan or verification checklist in the PR body. PRs merge to `staging` before the owner can test, so checkboxes in a closed PR are inaccessible in practice.
- Instead, add a `## Verification` section to the **linked GitHub issue** (as a comment or by editing the issue body). The issue stays open until `staging` is merged into `main`, giving the owner a live, tickable checklist during the staging window.
- The staging→main merge (and issue auto-close) serves as the implicit sign-off that verification passed.

### Agent Git Behaviour
- Local commits may be created without asking for confirmation.
- Before pushing to `origin`, always show a summary of what will be pushed and ask for explicit confirmation.

## Architecture

The solution follows a **Clean Architecture** with four projects:

### Domain (`src/Domain`)
Pure domain layer with no external dependencies.
- **Entities**: `League`, `Season`, `LeagueSeason`, `LeagueSeasonUser`, `Round`, `Match`, `Solve`, `Scramble`, `RoundStanding`, `LeagueSeasonStanding`, `User`. All implement `IIdentifiable` and use static `Create()` factory methods with `Guid.CreateVersion7()`.
- **ValueObjects**: `SolveResult` — a `readonly record struct` representing a timed result (ms), DNF (`-1`), or DNS (`-2`). Supports `ToString()`, `FromString()`, and implicit `int` conversion.
- **Scoring**: `AverageCalculator.CalculateAo5()` — WCA-rules Ao5 average (drops best/worst, DNF if >1 invalid solve).

### Application (`src/Application`)
Business logic via **MediatR** (CQRS). No EF Core dependencies — only repository interfaces.
- **Abstractions**: Repository interfaces in `Abstractions/Repositories/`. `IReadRepository<T>` and `IReadWriteRepository<T>` are the base generics; `IUnitOfWork` aggregates all repositories.
- **Requests**: Each feature has a `*Request` (IRequest) and `*RequestHandler` (IRequestHandler). Handlers receive `IUnitOfWork` via constructor injection.
- **CommandResult**: Used for write operations — `CommandResult.Ok()`, `CommandResult.Fail(field, message)`, `CommandResult.FailGeneral(message)`. Field-specific errors are surfaced to Razor Page `ModelState`.
- **Validation**: Custom data annotation attributes in `Validation/` (`NotEmptyGuidAttribute`, `SolveResultAttribute`, etc.).

### Infrastructure (`src/Infrastructure`)
EF Core + PostgreSQL (Npgsql), registered via `AddBldLeagueInfrastructure(connectionString)`.
- **AppDbContext**: Uses `IDbContextFactory<AppDbContext>` pattern.
- **UnitOfWork**: Lazily instantiates each repository from the shared `AppDbContext`. Supports `BeginTransactionAsync` / `CommitTransactionAsync` / `SaveAsync`.
- **Repositories**: `ReadRepositoryBase<T>` and `ReadWriteRepositoryBase<T>` provide generic implementations; entity-specific repos extend these.
- **Migrations**: Located in `src/Infrastructure/Migrations/`. `SolveResultConverter` handles `SolveResult` ↔ `int` persistence.

### Web (`src/Web`)
ASP.NET Core Razor Pages, registered via `AddRazorPages()`.
- **Authentication**: WCA OAuth2 (`https://www.worldcubeassociation.org/oauth`). On login, the WCA user's `wcaId` is looked up in the local DB; unregistered users are rejected. Claims: `NameIdentifier` (local Guid), `Name`, `Role` (Admin/User), `thumbnail`.
- **Authorization**: `[AdminOnly]` attribute (inherits `[Authorize(Roles = "Admin")]`) gates all admin pages.
- **Page structure**: Public pages under `Pages/`; admin CRUD pages under `Pages/Admin/`; error pages under `Pages/Error/`.
- **ViewModels**: In `ViewModels/`, separate from DTOs in `Application/Dtos/`.
- **AJAX handlers**: Razor Pages use named handler methods (`OnGetLeagues`, `OnGetRounds`, `OnGetUsers`) returning `JsonResult` for dynamic form population (e.g., cascading dropdowns on the Add Match page). These exist as legacy — avoid adding new ones (see UI principles below).

## Keeping CLAUDE.md Up to Date

After any significant change — adding a new entity, request handler, admin page, helper, or DTO — update the relevant tables and task recipes in the **File Map** and **Common Tasks** sections of this file. The file map should reflect the current state of the codebase at all times.

## Code Practices

### Don't Repeat Yourself
Duplicated logic must be extracted to a shared location before the second use is added. The right home depends on the layer:
- **Domain** (`src/Domain/Scoring/` or similar) — pure computation with no external dependencies (e.g., `AverageCalculator`).
- **Application** (`src/Application/Common/`) — logic that touches domain entities and/or repository interfaces but is shared across multiple handlers (e.g., `MatchSolvesProcessor`).
- **Web** — shared Razor partials or tag helpers for repeated UI fragments.

Never copy a block of logic from one handler/page to another. If you find yourself doing so, stop and extract first.

### Result Pattern — No Exceptions for Control Flow
Use `CommandResult` (and similar result types) to communicate success or failure from handlers to callers. Do **not** throw exceptions to signal expected failure conditions such as "entity not found", "validation failed", or "business rule violated".

Exceptions are reserved for truly exceptional situations: missing required configuration, failed database connection, unrecoverable infrastructure errors, etc.

Concretely:
- Handlers return `CommandResult.Ok()`, `CommandResult.Fail(field, message)`, or `CommandResult.FailGeneral(message)`.
- Pages inspect `result.Success` / `result.IsGeneralError` and route accordingly (add to `ModelState` or set `TempData`).
- Never `throw` to indicate that a user-facing operation could not be completed.

## UI Principles

### Plain, Simple Razor Pages
Keep the UI straightforward: standard Razor Pages with Bootstrap. Avoid custom CSS frameworks, heavy component libraries, or complex layouts. Pages should be boring — clarity and correctness over visual flair.

### Minimise JavaScript
Avoid JavaScript by default. Prefer server-side form submissions and page reloads. Only introduce JavaScript (including AJAX) when a feature genuinely cannot be built without it — for example, cascading dropdowns that would require an unreasonable number of round-trips or hidden fields. When JS is necessary, keep it inline on the page and as small as possible.

## Domain Concepts

- **Season** → contains **Rounds** and belongs to multiple **LeagueSeasons**.
- **League** → a division (e.g., "Liga A"). Multiple leagues run simultaneously within a season.
- **LeagueSeason** → junction of League + Season; holds the roster (`LeagueSeasonUser`). Users can be assigned a `SubleagueGroup` (integer) on their `LeagueSeasonUser` record to split the league into subleagues during a revenge period.
- **Season** has no start/end dates — it is just a `SeasonNumber`. "Latest season" (highest `SeasonNumber`) is used as the UI default in AddRound and AddMatch. LeagueSeasons are created manually (not auto-generated).
- **Match** → 1v1 within a Round + League. Each match has exactly 5 solves (`Match.SOLVES_PER_MATCH`) per player. Scoring: 1 point per solve won + 1 bonus point for best single. Scores are computed and stored at match creation time. Matches have a `MatchStatus` (Upcoming/InProgress/Finished) derived from the round's `StartDate`/`EndDate` relative to `DateTime.UtcNow`; scores and solve details are hidden in the UI until the match is Finished.
- **Scramble** → one per solve position (1–`Match.SOLVES_PER_MATCH`) per round; shared across all leagues in that round. Field `Notation` holds the move sequence. A round may have 0–5 scrambles.
- **RoundStanding** — standings per round per league; refreshed on demand via `RefreshRoundStandingsRequest` (single round) or `RefreshAllRoundStandingsRequest` (all finished rounds). Points: place 1–7 → 50–38 (step -2), place 8–44 → 37–1 (step -1).
- **LeagueSeasonStanding** — cumulative season standings; refreshed via `RefreshLeagueSeasonStandingsRequest` (single league season) or `RefreshAllLeagueSeasonStandingsRequest` (all league seasons).

## File Map

### Domain — key types

| Type | File |
|---|---|
| `League` entity | `src/Domain/Entities/League.cs` |
| `Season` entity | `src/Domain/Entities/Season.cs` |
| `LeagueSeason` entity | `src/Domain/Entities/LeagueSeason.cs` |
| `LeagueSeasonUser` entity | `src/Domain/Entities/LeagueSeasonUser.cs` |
| `Round` entity | `src/Domain/Entities/Round.cs` |
| `Match` entity | `src/Domain/Entities/Match.cs` |
| `Solve` entity | `src/Domain/Entities/Solve.cs` |
| `Scramble` entity | `src/Domain/Entities/Scramble.cs` |
| `RoundStanding` entity | `src/Domain/Entities/RoundStanding.cs` |
| `LeagueSeasonStanding` entity | `src/Domain/Entities/LeagueSeasonStanding.cs` |
| `User` entity | `src/Domain/Entities/User.cs` |
| `SolveResult` value object | `src/Domain/ValueObjects/SolveResult.cs` |
| `AverageCalculator` (Ao5 logic) | `src/Domain/Scoring/AverageCalculator.cs` |
| `IIdentifiable` interface | `src/Domain/Interfaces/IIdentifiable.cs` |

### Application — infrastructure

| Type | File |
|---|---|
| `IUnitOfWork` (aggregates all repos) | `src/Application/Abstractions/Repositories/IUnitOfWork.cs` |
| `IReadRepository<T>` | `src/Application/Abstractions/Repositories/IReadRepository.cs` |
| `IReadWriteRepository<T>` | `src/Application/Abstractions/Repositories/IReadWriteRepository.cs` |
| `CommandResult` | `src/Application/Common/CommandResult.cs` |
| `ImportResult` (import aggregate result) | `src/Application/Common/ImportResult.cs` |
| `ImportRowResult` (per-row import result) | `src/Application/Common/ImportRowResult.cs` |
| `MatchSolvesProcessor` (shared match logic) | `src/Application/Common/MatchSolvesProcessor.cs` |
| `ScrambleDto` (scramble data transfer) | `src/Application/Queries/Rounds/GetScrambles/ScrambleDto.cs` |
| `MatchExportRowDto` (match CSV export row) | `src/Application/Queries/Matches/GetMatchExport/MatchExportRowDto.cs` |
| `SolveDto` (solve input for create/edit) | `src/Application/Queries/Matches/GetMatchDetailsById/SolveDto.cs` |
| Validation attributes | `src/Application/Validation/` |
| MediatR / DI registration | `src/Application/ServiceCollectionExtensions.cs` |

### Application — repository interfaces

| Interface | File |
|---|---|
| `ILeagueRepository` | `src/Application/Abstractions/Repositories/ILeagueRepository.cs` |
| `ISeasonRepository` | `src/Application/Abstractions/Repositories/ISeasonRepository.cs` |
| `ILeagueSeasonRepository` | `src/Application/Abstractions/Repositories/ILeagueSeasonRepository.cs` |
| `ILeagueSeasonUserRepository` | `src/Application/Abstractions/Repositories/ILeagueSeasonUserRepository.cs` |
| `IRoundRepository` | `src/Application/Abstractions/Repositories/IRoundRepository.cs` |
| `IMatchRepository` | `src/Application/Abstractions/Repositories/IMatchRepository.cs` |
| `ISolveRepository` | `src/Application/Abstractions/Repositories/ISolveRepository.cs` |
| `IScrambleRepository` | `src/Application/Abstractions/Repositories/IScrambleRepository.cs` |
| `IRoundStandingRepository` | `src/Application/Abstractions/Repositories/IRoundStandingRepository.cs` |
| `ILeagueSeasonStandingRepository` | `src/Application/Abstractions/Repositories/ILeagueSeasonStandingRepository.cs` |
| `IUserRepository` | `src/Application/Abstractions/Repositories/IUserRepository.cs` |

### Application — commands (write operations, by feature)

| Feature area | Folder |
|---|---|
| League create/update/delete/import | `src/Application/Commands/Leagues/` |
| Season create/edit/delete/import | `src/Application/Commands/Seasons/` |
| LeagueSeason create/delete/import | `src/Application/Commands/LeagueSeasons/` |
| LeagueSeasonUser add/remove/import/set-group | `src/Application/Commands/LeagueSeasonUsers/` |
| Round create/update/delete/import + scramble update | `src/Application/Commands/Rounds/` |
| Match create/edit/delete/import | `src/Application/Commands/Matches/` |
| User create/update/delete/import | `src/Application/Commands/Users/` |
| Refresh round standings (single + refresh-all) | `src/Application/Commands/RoundStandings/Refresh/` and `RefreshAll/` |
| Refresh season standings (single + refresh-all) | `src/Application/Commands/LeagueSeasonStandings/Refresh/` and `RefreshAll/` |

### Application — queries (read operations, by feature)

| Feature area | Folder |
|---|---|
| League queries + DTOs | `src/Application/Queries/Leagues/` |
| Season queries + DTOs | `src/Application/Queries/Seasons/` |
| LeagueSeason queries + DTOs | `src/Application/Queries/LeagueSeasons/` |
| Round queries + DTOs (incl. `ScrambleDto`, `RoundSummaryDto`) | `src/Application/Queries/Rounds/` |
| Match queries + DTOs (incl. `SolveDto`, `MatchDetailsDto`, `MatchExportRowDto`) | `src/Application/Queries/Matches/` |
| User queries + DTOs (incl. `LeagueSeasonUserDto` for roster queries) | `src/Application/Queries/Users/` |

### Infrastructure

| Type | File |
|---|---|
| `AppDbContext` | `src/Infrastructure/Context/AppDbContext.cs` |
| `UnitOfWork` | `src/Infrastructure/Repositories/UnitOfWork.cs` |
| `ReadRepositoryBase<T>` | `src/Infrastructure/Repositories/ReadRepositoryBase.cs` |
| `ReadWriteRepositoryBase<T>` | `src/Infrastructure/Repositories/ReadWriteRepositoryBase.cs` |
| `SolveResultConverter` (EF value converter) | `src/Infrastructure/Converters/SolveResultConverter.cs` |
| `EnsureMigratedHelper` (auto-migration on startup) | `src/Infrastructure/Helpers/EnsureMigratedHelper.cs` |
| EF entity configurations | `src/Infrastructure/Configuration/` |
| Migrations | `src/Infrastructure/Migrations/` |
| DI registration | `src/Infrastructure/ServiceCollectionExtensions.cs` |

### Web

| Type | File |
|---|---|
| App entry point + DI wiring | `src/Web/Program.cs` |
| WCA OAuth setup | `src/Web/Auth/AuthenticationExtensions.cs` |
| WCA API response models | `src/Web/Auth/WcaUser.cs`, `WcaUserAvatar.cs`, `WcaUserResponse.cs` |
| `[AdminOnly]` attribute | `src/Web/Attributes/AdminOnlyAttribute.cs` |
| Shared layout | `src/Web/Pages/Shared/_Layout.cshtml` |
| `CsvHelper` (CSV builder, UTF-8 BOM) | `src/Web/Helpers/CsvHelper.cs` |
| `CsvParser` (CSV `IFormFile` parser) | `src/Web/Helpers/CsvParser.cs` |
| `EnvironmentBadgeOptions` (optional navbar env label) | `src/Web/Options/EnvironmentBadgeOptions.cs` |
| `MatchStatus` enum (Upcoming/InProgress/Finished) | `src/Web/ViewModels/MatchStatus.cs` |
| ViewModels | `src/Web/ViewModels/` |

### Web — public pages

| Page | Files |
|---|---|
| Home / standings index | `src/Web/Pages/Index.cshtml[.cs]` |
| View league season standings | `src/Web/Pages/Leagues/ViewLeague.cshtml[.cs]` |
| View round results | `src/Web/Pages/Rounds/ViewRound.cshtml[.cs]` |
| Match list | `src/Web/Pages/Matches/MatchList.cshtml[.cs]` |
| Match detail | `src/Web/Pages/Matches/ViewMatch.cshtml[.cs]` |
| About / rules | `src/Web/Pages/About/About.cshtml[.cs]` |
| Season 2 guidelines | `src/Web/Pages/About/Guidelines.cshtml[.cs]` |
| Season 1 guidelines (archived) | `src/Web/Pages/About/GuidelinesSeason1.cshtml[.cs]` |

### Web — admin pages

| Page | Files |
|---|---|
| Admin dashboard | `src/Web/Pages/Admin/Admin.cshtml[.cs]` |
| Leagues list / add / edit / delete / import / export | `src/Web/Pages/Admin/Leagues/` |
| Seasons list / add / edit / delete / import / export | `src/Web/Pages/Admin/Seasons/` |
| LeagueSeasons list / add / delete / import / export | `src/Web/Pages/Admin/LeagueSeasons/AllLeagueSeasons.cshtml[.cs]`, `AddLeagueSeason.cshtml[.cs]` |
| Edit LeagueSeason roster (+ user CSV import/export) | `src/Web/Pages/Admin/LeagueSeasons/EditLeagueSeason.cshtml[.cs]` |
| Matches list / add / edit | `src/Web/Pages/Admin/Matches/` |
| Users list / add / edit | `src/Web/Pages/Admin/Users/` |

## Common Tasks

**Add a new entity:**
1. `src/Domain/Entities/NewEntity.cs` — implement `IIdentifiable`, add `Create()` factory
2. `src/Application/Abstractions/Repositories/INewEntityRepository.cs` — extend `IReadWriteRepository<T>`
3. `src/Application/Abstractions/Repositories/IUnitOfWork.cs` — add property
4. `src/Infrastructure/Configuration/NewEntityConfiguration.cs` — EF config
5. `src/Infrastructure/Repositories/NewEntityRepository.cs` — extend `ReadWriteRepositoryBase<T>`
6. `src/Infrastructure/Repositories/UnitOfWork.cs` — wire up lazy property
7. `src/Infrastructure/Context/AppDbContext.cs` — add `DbSet<T>`
8. Run `dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/Web`

**Add a new request/handler:**
1. Create folder `src/Application/Requests/[Feature]/[Operation]/`
2. Add `*Request.cs` (implements `IRequest<TResponse>`)
3. Add `*RequestHandler.cs` (implements `IRequestHandler<TRequest, TResponse>`, receives `IUnitOfWork`)
4. Call from a Razor Page via `_mediator.Send(new *Request(...))`

**Add a new admin page:**
1. Create `src/Web/Pages/Admin/[Feature]/PageName.cshtml` + `.cshtml.cs`
2. Decorate the PageModel with `[AdminOnly]`
3. Inject `IMediator` and call handlers in `OnGet`/`OnPost`

**Add CSV export to an admin page:**
1. Add a query request (e.g. `GetMatchExportRequest`) that returns `IEnumerable<MatchExportRowDto>` (or similar DTO).
2. In the handler, project entity data into the DTO.
3. In the page's `OnGetExport` handler, call `CsvHelper.BuildCsv(headers, rows)` and return `File(bytes, "text/csv", "filename.csv")`.

**Add CSV import to an admin page:**
1. Add `Import*Request` and `Import*RequestHandler` under the feature folder; the handler returns `ImportResult`.
2. Handlers process each row independently: use `ImportRowResult.Ok(row, msg)` / `ImportRowResult.Fail(row, msg)` — never throw for row-level errors.
3. In the page, parse the uploaded `IFormFile` with `CsvParser.ParseAsync()`, skip the header row, map fields to the request model, and pass to the handler.
4. Render `ImportResult.RowResults` in the page to show per-row success/failure feedback.
