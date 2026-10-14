# Backend API

ASP.NET Core API targeting .NET 10.

## Run locally

From the repository root:

```sh
dotnet run --project backend/API
dotnet test ProjectPortal.slnx
```

Development uses a seeded, in-memory database by default. Data resets when the
API restarts. Set `Database__SeedDevelopmentData=false` to disable seeding.

To use PostgreSQL, set `Database__Provider=Postgres` and
`ConnectionStrings__DefaultConnection`. The schema is managed by
[SQL migrations](../db/migrations), not by EF migrations.

## Code structure

Requests normally flow through `Controller → Service → Repository`. Controllers
handle HTTP, services handle operations and validation, and repositories handle
data access. DTOs define request and response shapes.

The [Orchestrations directory](API/Orchestrations/README.md) is reserved for
workflows that need CRUD calls to multiple services. None are used currently.

`ApiExceptionHandler` returns problem details for errors, including a trace ID.
Validation, missing resources, conflicts, and unexpected failures have separate
status codes. Internal error details are logged rather than returned to clients.

See [API.http](API/API.http) for example requests and routes.

Authentication and per-user database access are still being developed. Until then,
the API only starts in Development or Testing.
