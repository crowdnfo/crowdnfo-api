# crowdnfo-api

Backend HTTP API for the **crowdNFO** project.


## Solution layout

```
Web.Api  ──>  Infrastructure  ──>  Application  ──>  Domain  ──>  SharedKernel
(HTTP)        (DB/Auth/EF)         (use cases)       (rules)      (primitives)
```

- **SharedKernel** — common DDD abstractions (`Entity`, `Result`, `Error`, domain-event base).
- **Domain** — entities, enums, domain events and domain errors. No framework code.
- **Application** — use cases (commands/queries), abstractions, cross-cutting concerns (logging, validation).
- **Infrastructure** — EF Core / PostgreSQL, JWT authentication, permission authorization, Serilog.
- **Web.Api** — minimal-API endpoints, middleware, composition root.
- **tests/ArchitectureTests** — enforces the inward-only dependency direction.

## Tech stack

- .NET 10, minimal APIs
- EF Core + PostgreSQL (snake_case, migrations in `Infrastructure/Database/Migrations`)
- JWT bearer authentication with permission-based authorization
- Serilog structured logging, shipped to Seq

## Running locally

```bash
docker compose up
```

This starts the API, a PostgreSQL container (database `crowdnfo`), and Seq.

- API: http://localhost:5000
- Seq (log search/analysis): http://localhost:8081

Application secrets (JWT secret, connection string) are read from configuration —
override them via user secrets or environment variables for non-development environments.

## Build & test

```bash
dotnet build CrowdNfo.Api.slnx
dotnet test CrowdNfo.Api.slnx
```
