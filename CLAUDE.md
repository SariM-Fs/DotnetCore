# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

An ASP.NET Core (net8.0) Web API for booking EV charging spots. Drivers browse stations, claim a
free charging spot, run a session, and pay for it. Admins manage stations, spots and amenities.

## Commands

Run all commands from the `server/` folder (this file's directory).

```
# Build
dotnet build    # builds final-project-API.sln at the server/ root

# Run the API
dotnet run --project final_project_Api

# Apply EF Core migrations (DbContext lives in Data, runnable app is Api)
dotnet ef database update -p final_project_Data -s final_project_Api

# Add a new migration
dotnet ef migrations add <Name> -p final_project_Data -s final_project_Api

# Run all tests
dotnet test final_project_Test

# Run a single test (by fully-qualified name or filter)
dotnet test final_project_Test --filter "FullyQualifiedName~ChargingSpotServiceTests"
dotnet test final_project_Test --filter "DisplayName~StartSession_WhenSpotAvailable_Succeeds"
```

First-time setup (secrets are not committed — see README.md for full details):

```
dotnet user-secrets set "Jwt:Key" "<32+ char random string>" --project final_project_Api
```

The database is PostgreSQL (Npgsql provider). The concurrency integration test
(`ConcurrencyIntegrationTests.cs`) needs the same local PostgreSQL used by the app
(`localhost:5432`, see `final_project_Api/appsettings.Development.json`) with migrations applied.

## Architecture

Five projects in one solution (`final-project-API.sln`, at the `server/` root), dependencies flowing one way:

```
final_project_Core      Entities, DTOs, Enums, OperationResult<T>, service/repository interfaces
final_project_Data      DbContext, EF Core fluent configuration, Repositories, Migrations
final_project_Service   Business logic (Services), AutoMapper profiles
final_project_API       Controllers, Middleware, Program.cs, Auth (JWT), BackgroundServices
final_project_Test      xUnit + Moq tests
```

`Core` depends on nothing (not even EF Core). `Data` and `Service` depend only on `Core`. `API`
depends on `Service` and `Core`, and references `Data` solely to wire up the DbContext/repositories
in `Program.cs` — controllers never reference `Data` types directly. All cross-project references
go through interfaces defined in `Core` (`Core/Interface` for services, `Core/Repositories` for
repositories), and `Program.cs` is the single place that binds interface → implementation.

**Result flow, request → response:** Controller → Service (returns `OperationResult<T>`) →
Repository → `AppDbContext`. Controllers stay thin: they extract the caller's driver id from the
JWT claims, call the service, and translate `OperationResult<T>.Status` to an HTTP status via a
private `ToActionResult` helper (`Success`→200, `NotFound`→404, `Conflict`→409, `ValidationError`→
400, `Forbidden`→403). This mapping is duplicated per-controller rather than shared — follow the
existing pattern in a controller (e.g. `SessionsController`) when adding a new one.

**Optimistic concurrency on `ChargingSpot`:** a spot is the limited resource multiple drivers
compete for. `ChargingSpot.Version` (`uint`, `IsRowVersion()`) is mapped by Npgsql to PostgreSQL's
`xmin` system column and acts as the concurrency token. `ChargingSpotService.StartSessionAsync`/`EndSessionAsync` read-check-write within one
DbContext, then catch `DbUpdateConcurrencyException` specifically (never a generic `Exception`) and
turn it into `OperationResult.Conflict`. The early "is it Available?" check is just a fast path —
the row version is the actual source of correctness under a race. Don't remove or generalize this
catch when touching spot/session code. See `final_project_Test/Tests/ConcurrencyIntegrationTests.cs`
for the proof (two real `DbContext` instances racing to occupy the same spot).

**Enums serialize as strings**: `SpotStatus`, `PaymentMethod`, etc. use
`JsonStringEnumConverter` (registered in `Program.cs`), so API request/response bodies use names
like `"Available"`/`"CreditCard"`, not integers.

**Session timeout sweep**: `BackgroundServices/SessionTimeoutHostedService` periodically calls
`ChargingSpotService.EndExpiredSessionsAsync` to auto-end sessions left active too long
(configured via `SessionTimeoutOptions`).

**Global error handling**: `ExceptionHandlingMiddleware` wraps the entire pipeline (registered
before `CorrelationIdMiddleware` in `Program.cs`) so any unhandled exception becomes uniform JSON.

**Auth**: JWT bearer auth (`Auth/JwtTokenGenerator.cs`). The signing key comes from config
(`Jwt:Key`, via User Secrets in dev / env vars in prod) — never hardcode or commit it.
`[Authorize(Roles = "Admin")]` gates station/spot/amenity mutations; regular drivers (role `User`)
browse, start/end sessions, and pay. The caller's driver id is read from
`ClaimTypes.NameIdentifier`, never taken from the request body.

**Dev data seeding**: `Program.cs` seeds amenities, 3 stations (6 spots each, mixed statuses) and
optionally an Admin driver (from `AdminSeed:Email`/`AdminSeed:Password` config) on startup if the
tables are empty — convenience only, not a migration.
