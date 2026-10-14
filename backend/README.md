# Backend

The backend is an ASP.NET Core controller API targeting .NET 10.

```text
backend/
├── API/                 # Controllers, orchestrations, services, repositories, models, DTOs
└── tests/
    └── API.Tests/       # xUnit tests organized by layer and production class
```

Run these commands from the repository root:

```sh
dotnet run --project backend/API
dotnet test ProjectPortal.slnx
```

Open the repository root in VS Code and select **Project Portal API (Swagger)**
to build, debug, and open Swagger automatically.

The schema source is [db/migrations](../db/migrations); database tests and
infrastructure have their own root-level directories. No EF migrations are used.

## Responsibilities and exceptions

```text
Controller → Service → Repository → ApplicationDbContext
Controller → Orchestration → Services → Their own repositories
```

Repositories own EF queries and persistence. They wrap failures, including
cancellation, in `RepositoryException` with the original exception attached.
Services own validation and domain rules, depend only on their own repository, and wrap
dependency failures in `ServiceException`. Every service operation runs inside a
try/catch block. Known service exceptions already contain a cause and propagate
without an extra wrapper. The only custom exception types are `RepositoryException`,
`ServiceException`, and `OrchestrationException`; each constructor requires an inner
exception. Error classification is metadata rather than an exception subclass.

Orchestrations coordinate multiple different services and never access repositories
or implement validation rules. They wrap service failures and cancellation in
`OrchestrationException`, preserving classification and the attached cause. Simple
operations go directly from a controller to its service.

Project creation and updates, profile updates, inquiry creation, and saved-project
operations use orchestrations. TopicService validates topic references, StudentService
and SupervisorService validate their own profile references, and ProjectService
validates project ownership. Orchestrations call these operations before the service
that performs the write. Database constraints handle concurrent changes before persistence.
Pure DTO mapping lives in `DTOs/Mapping`.

`Middleware/ApiExceptionHandler` surfaces the basic service/orchestration exception
as JSON problem details: 400 for validation, 404 for missing resources, 409 for
conflicts, 499 for cancellation, and 500 for unexpected failures. It avoids writing
a response body when the client has already disconnected. Other responses contain
a request path and trace ID. Unexpected failures and conflicts are logged with
attached exceptions; internal database details are not returned to clients.

## Project lookups

Each project GET endpoint has one purpose and accepts no combined query filters:

| Route | Operation |
| --- | --- |
| `/api/v1/projects` | GetProjects |
| `/api/v1/projects/{projectId}` | GetProjectById |
| `/api/v1/projects/supervisors/{supervisorId}` | GetProjectsBySupervisorId |
| `/api/v1/projects/topics/{topicId}` | GetProjectsByTopicId |
| `/api/v1/projects/statuses/{status}` | GetProjectsByStatus |

POST on `/api/v1/projects` creates a project; PUT and DELETE on the ID route update
and delete it. POST returns 201 with a Location header; successful PUT/DELETE return
204. Collection lookups return an empty array when there are no matches.
See [API.http](API/API.http) for sample requests.

## Development and validation

Development defaults to an ephemeral InMemory database with 4 supervisors,
5 students, 8 topics, 9 projects, 8 inquiries, and 10 saved-project relationships.
Restarting resets it; `Database__SeedDevelopmentData=false` disables seeding.
Set `Database__Provider=Postgres` and `ConnectionStrings__DefaultConnection` to use
an existing PostgreSQL/Supabase database. Startup never applies schema changes.
Authentication and per-user RLS context are still pending; startup is restricted to
Development/Testing. Signed attachment uploads and metadata endpoints are deferred.

In VS Code with C# Dev Kit, reload the window after moving projects, then select
**Testing → Refresh Tests**. The workspace selects the root solution. C# tests cover
HTTP behavior, dedicated lookups, exception chains, cancellation, relationship
validation, orchestration call order, service dependency boundaries, and concurrent
saves. Each controller, orchestration, service, and repository has a matching test
file. Npgsql model checks do not open a database
connection; SQL/RLS tests still require Supabase.
