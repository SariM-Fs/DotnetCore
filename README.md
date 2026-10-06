# EV Charging — Server

An ASP.NET Core Web API for booking EV charging spots. Drivers browse stations, claim a free
charging spot, run a session, and pay for it. Admins manage stations, spots and amenities.

## Architecture

Five projects in one solution, dependencies flowing one way:

```
final-project-API.sln
  final_project_Core      // Entities, DTOs, Enums, service/repository interfaces
  final_project_Data       // DbContext, EF Core configuration, Repositories, Migrations
  final_project_Service    // Business logic (Services), AutoMapper profiles
  final_project_API        // Controllers, Middleware, Program.cs
  final_project_Test       // xUnit + Moq tests
```

`Core` has no dependency on anything else (not even EF Core). `Data` and `Service` depend only on
`Core`. `API` depends on `Service` and `Core`, and references `Data` solely to register its
DbContext/repositories in `Program.cs`.

## The limited resource: `ChargingSpot`

A **charging spot** (`ChargingSpot`) at a station is the resource multiple drivers compete for.
Only one active session can occupy a spot at a time.

The contention window is the classic one: a request reads the spot's status, checks it's
`Available`, then writes back `Occupied`. Between the read and the write, another request can do
the exact same thing — without protection, two drivers can both "successfully" start a session on
the same physical spot.

**This project uses optimistic concurrency** to close that window:

- `ChargingSpot.Version` is a concurrency token mapped to PostgreSQL's built-in `xmin` system
  column — the database changes it automatically on every update, and EF Core includes the value
  the app *read* in the `WHERE` clause of the generated `UPDATE`.
- If a second request already changed the row in between, that `UPDATE` matches zero rows and EF
  Core throws `DbUpdateConcurrencyException`.
- `ChargingSpotService.StartSessionAsync` catches that specific exception (not a generic
  `Exception`) and returns a `Conflict` result, which `SessionsController` turns into an HTTP
  **409** with a clear message ("This spot was just taken by another driver...").
- The business check (is the spot `Available`?) and the save happen in the same request/DbContext,
  so the concurrency token is the actual source of correctness under a race — the early check is
  just a fast path for the common case.

This is proven by `final_project_Test/Tests/ConcurrencyIntegrationTests.cs`, which opens **two
separate, real `DbContext` instances**, has both load the same spot, both set it to `Occupied`,
and asserts the first save succeeds while the second throws `DbUpdateConcurrencyException`.
`final_project_Test/Tests/ChargingSpotServiceTests.cs` covers the same logic with mocked
repositories, for both the happy path and the rejected path.

## Running it locally

### 1. Database

This project targets **PostgreSQL** (via the Npgsql EF Core provider). The easiest way to get a
local server is Docker:

```
docker run -d --name evcharging-db -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres:16
```

The development connection string lives in `final_project_Api/appsettings.Development.json` and
matches that container:

```
Host=localhost;Port=5432;Database=EVChargingDb;Username=postgres;Password=postgres
```

In production (e.g. Render), set `DATABASE_URL` or `ConnectionStrings__DefaultConnection`. Either
the `Host=...;Database=...` format above or a `postgres://user:password@host/db` URL (what Render
shows) works — `Program.cs` converts the URL form for Npgsql. See [Deploying to Render](#deploying-to-render).

There is one migration, `InitialCreate` (the full schema, including `ChargingSpot.Version` mapped
to PostgreSQL's `xmin` as the concurrency token). Locally, apply it from the `server/` folder:

```
dotnet ef database update -p final_project_Data -s final_project_Api
```

(`-p` points at the project that owns the `DbContext`, `-s` at the runnable API project — they're
different projects in this layout, so both flags are required.)

### 2. Secrets (JWT key)

The JWT signing key is **not** committed anywhere in this repo — `appsettings.json` only holds the
non-secret `Issuer`/`Audience`. Before running, set it via User Secrets:

```
dotnet user-secrets set "Jwt:Key" "<any random string, 32+ characters>" --project final_project_Api
```

(Optional) to get the Admin account seeded automatically on first run, also set:

```
dotnet user-secrets set "AdminSeed:Email" "admin@example.com" --project final_project_Api
dotnet user-secrets set "AdminSeed:Password" "<a password>" --project final_project_Api
```

### 3. Run

```
dotnet run --project final_project_Api
```

On first run it seeds 3 stations (with 6 spots and a couple of amenities each) so the app has data
immediately — no manual database setup beyond the migration step above. Swagger UI is available at
`/swagger` in development, with a "Authorize" button for pasting a JWT.

### 4. Tests

```
dotnet test final_project_Test
```

The concurrency integration test needs the PostgreSQL server from step 1 running, with migrations
applied.

## Demo users

- **Admin** — whatever email/password you set as `AdminSeed:Email` / `AdminSeed:Password` above
  (seeded automatically on first run if both are set).
- **Regular driver** — register any account via `POST /api/auth/register` (or the client's
  Register screen); new accounts always get the `User` role.

Admins can PATCH station/spot status and create/delete stations & amenities
(`[Authorize(Roles = "Admin")]`); regular drivers can browse, start/end sessions, and pay.

## Deploying to Render

The API ships as a Docker image (`Dockerfile` at the `server/` root). Outside Development, the app
applies pending migrations itself on startup, so no manual `dotnet ef` step is needed there.

1. **PostgreSQL** — create a Render PostgreSQL instance and copy its *Internal Database URL*.
2. **API** — create a Web Service from this repo, runtime **Docker**, root directory `server`
   (if the repo root isn't `server/`). Environment variables:

   | Variable | Value |
   |---|---|
   | `DATABASE_URL` | the Internal Database URL from step 1 |
   | `Jwt__Key` | a random string, 32+ characters |
   | `AdminSeed__Email` / `AdminSeed__Password` | optional — seeds the Admin account |
   | `Cors__AllowedOrigins` | the deployed client's URL, e.g. `https://ev-client.onrender.com` (comma-separate several) |

3. **Client** — create a Static Site from the client repo: build command `npm install && npm run build`,
   publish directory `dist`, and env var `VITE_API_BASE` = `https://<your-api>.onrender.com/api`.
   Vite bakes this in at build time, so redeploy the client after changing it.
