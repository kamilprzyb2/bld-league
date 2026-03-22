# BldLeague

Live at **[bldleague.pl](http://bldleague.pl/)**

A web application for managing a BLD (Blindfolded) speedcubing league. Tracks seasons, leagues, rounds, matches, and player standings. The UI is in Polish.

Built with **ASP.NET Core 10** (Razor Pages), **PostgreSQL**, and **WCA OAuth** authentication.

## Features

- League and season management (multiple leagues run simultaneously within a season)
- 1v1 match tracking with WCA-rules Ao5 scoring
- Scramble management (shared across leagues per round)
- Round and season standings with automatic refresh
- Admin panel for full CRUD over all entities
- CSV import/export for bulk data management
- Authentication via [World Cube Association](https://www.worldcubeassociation.org/) OAuth — only pre-registered WCA members can log in

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/) — or Docker (see below)

## Running Locally

### With Docker (recommended)

```bash
docker compose up --build
```

This starts the app on port 8080 and a PostgreSQL 17 database. Migrations are applied automatically on startup.

### Without Docker

1. **Configure the connection string** in `src/Web/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "Default": "Host=localhost;Database=bldleague;Username=postgres;Password=postgres"
     }
   }
   ```

2. **Configure WCA OAuth** credentials (obtain from [WCA Developers](https://www.worldcubeassociation.org/oauth/applications)) in `src/Web/appsettings.json`:

   ```json
   {
     "WCA": {
       "ClientId": "<your-client-id>",
       "ClientSecret": "<your-client-secret>"
     }
   }
   ```

3. **Run the app:**

   ```bash
   dotnet run --project src/Web
   ```

   The database is migrated automatically on startup.

## Configuration Reference

All settings are configured in `src/Web/appsettings.json`.

| Key | Description |
|---|---|
| `ConnectionStrings:Default` | PostgreSQL connection string |
| `WCA:ClientId` | WCA OAuth application client ID |
| `WCA:ClientSecret` | WCA OAuth application client secret |
| `SuperAdmin:WcaId` | WCA ID of the initial admin user (optional, seeded on first run) |
| `SuperAdmin:FullName` | Display name of the initial admin user (optional) |

For Docker/production, use environment variables with double-underscore notation, e.g. `ConnectionStrings__Default`.

## Authentication

Login is handled via WCA OAuth. Users must be **pre-registered** in the database by an admin before they can log in — unknown WCA members are rejected. If `SuperAdmin` is configured, that user is seeded automatically on first startup as the initial admin.

Roles:
- **Admin** — full access to the admin panel
- **User** — read-only public access (no admin panel)

## Architecture

The solution follows Clean Architecture with four projects:

```
src/
├── Domain/         — entities, value objects, scoring logic (no dependencies)
├── Application/    — CQRS handlers via MediatR, repository interfaces, result types
├── Infrastructure/ — EF Core + PostgreSQL, repository implementations, migrations
└── Web/            — ASP.NET Core Razor Pages, WCA OAuth, admin panel
```

Key patterns:
- **CQRS via MediatR** — every operation is a typed `*Request` / `*RequestHandler`
- **Repository + Unit of Work** — all data access behind interfaces
- **Result pattern** — `CommandResult.Ok()` / `.Fail()` instead of exceptions for control flow
- **Auto-migrations** — `EnsureMigratedHelper` applies pending migrations on startup

## Database Migrations

```bash
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/Web
```

## Development

```bash
dotnet build BldLeague.slnx
dotnet run --project src/Web
```
