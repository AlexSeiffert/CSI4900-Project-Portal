# CSI4900 Project Portal

A web-based platform for discovering and managing CSI4900 honours projects.

The CSI4900 Project Portal aims to provide students with a centralized platform
to discover honours projects and supervisors, while providing supervisors with
tools to manage their projects and availability.

## Team

### Developers

- Alexander Seiffert
- Owen Ritchie

### Supervisor

- Stephane Some

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

The backend API and database schema are under development. The frontend is planned
and has a reserved directory for future implementation.

## Repository Structure

```text
.
├── backend/
│   ├── API/                 # ASP.NET Core Web API (.NET 10)
│   └── tests/API.Tests/     # C# integration and mapping tests
├── frontend/                # Future UI application
├── db/
│   ├── migrations/          # Authoritative SQL schema migrations
│   ├── seed/                # Database seed scripts
│   └── tests/               # PostgreSQL / RLS tests
├── infra/                   # Terraform infrastructure
├── .github/                 # CI workflows and PR template
├── .vscode/                 # Shared debug and build configuration
└── ProjectPortal.slnx       # .NET solution
```

Each application owns its code, tests, dependencies, and configuration. Database
schema and infrastructure are shared concerns managed in `db/` and `infra/`.
Repository-wide editor settings and tool versions remain at the root.

## Local Development

Open the repository root in VS Code. With the C# extension installed, select
**Project Portal API (Swagger)** in Run and Debug and press **F5**. This builds the
backend and opens Swagger on an automatically assigned port.

From the repository root, use the .NET 10 SDK to run, build, or test:

```sh
dotnet run --project backend/API
dotnet build ProjectPortal.slnx
dotnet test ProjectPortal.slnx
```

Local API development and C# tests use InMemory and do not require database
migrations. PostgreSQL mappings target the domain schema developed separately;
those domain migrations must be applied before enabling PostgreSQL/Supabase.
Database schema changes are excluded from this API change.

See the [backend guide](backend/README.md) for configuration, architecture, and endpoints.
The [frontend directory](frontend/README.md) is reserved for the future UI.

## Documentation

Detailed project documentation is maintained in the
[Project Wiki](../../wiki).

## Project Management

Development tasks and project progress are tracked using GitHub Issues and the
project Kanban board.
