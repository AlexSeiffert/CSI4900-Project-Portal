# uoProjects

A web-based platform for discovering and managing CSI4900 honours projects.

uoProjects gives students one place to discover honours projects and supervisors,
and gives supervisors tools to manage their projects and availability.

## Team

### Developers

- [Alexander Seiffert](https://github.com/AlexSeiffert)
- [Owen Ritchie](https://owenritchie.ca/?utm_source=uoproject)

### Supervisor

- [Stéphane Somé](https://www.uottawa.ca/faculty-engineering/school-electrical-engineering-computer-science/directory/stephane-soteg-some)

## Project Goals

### Students

- Discover available honours projects and supervisors
- Search and filter projects
- Save projects of interest
- Receive project recommendations based on interests
- Express interest in projects

### Supervisors

- Manage their profile and research interests
- Create and manage project listings
- Set availability
- Manage student interest
- Archive projects

## Project Status

The backend API and database schema are under development. The frontend is a
.NET 10 Blazor WebAssembly app (MudBlazor and Tailwind) with the sign-in page, the admin
dashboard, and student and supervisor home pages.

| Part | Stack |
| --- | --- |
| Frontend | Blazor WebAssembly, MudBlazor, Tailwind CSS |
| Backend | ASP.NET Core Web API (.NET 10), EF Core |
| Database and auth | Supabase (PostgreSQL with row-level security) |
| Infrastructure | Terraform |

## Repository Structure

```text
.
├── backend/
│   ├── API/                 # ASP.NET Core Web API (.NET 10)
│   └── tests/API.Tests/     # C# integration and mapping tests
├── frontend/
│   └── uoProjects/          # Blazor WebAssembly app with MudBlazor and Tailwind (.NET 10)
├── db/
│   ├── migrations/          # Authoritative SQL schema migrations
│   ├── seed/                # Database seed scripts
│   └── tests/               # PostgreSQL / RLS tests
├── infra/                   # Terraform infrastructure
├── .github/                 # CI workflows and PR template
├── .vscode/                 # Shared debug and build configuration
└── uoProjects.slnx          # .NET solution
```

Each application owns its code, tests, dependencies, and configuration. Database
schema and infrastructure are shared concerns managed in `db/` and `infra/`.
Repository-wide editor settings and tool versions remain at the root.

## Local Development

Open the repository root in VS Code, select **uoProjects (Full Stack)** in Run and Debug,
and press **F5**. It starts the API, the frontend at `http://localhost:5270`, and the Tailwind
watcher. Configure the root `.env` as described in the backend guide first.

Or from the repository root with the .NET 10 SDK:

```sh
# API against live Supabase (https://localhost:7049)
dotnet run --project backend/API --launch-profile https-supabase
```

```sh
# Frontend with hot reload (http://localhost:5270)
dotnet watch --project frontend/uoProjects --launch-profile http-watch
```

```sh
# Compile everything without running it
dotnet build uoProjects.slnx
```

```sh
# Run the C# tests (in-memory, no network needed)
dotnet test uoProjects.slnx
```

The backend launch profile uses Supabase/PostgreSQL and requires the domain
migrations to be applied. C# tests use InMemory and do not require database
migrations.

See the [backend guide](backend/README.md) for configuration, architecture, and endpoints.
See the [frontend guide](frontend/README.md) for the UI structure and run commands.

## Documentation

Detailed project documentation is maintained in the
[Project Wiki](../../wiki).

## Project Management

Development tasks and project progress are tracked using GitHub Issues and the
project Kanban board.
