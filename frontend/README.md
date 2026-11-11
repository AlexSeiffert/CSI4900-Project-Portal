# Frontend

The UI is a .NET 10 Blazor Web App using MudBlazor and global Interactive
Server rendering. The frontend runs as a separate application from the backend API.

## Folder structure

```text
frontend/
├── uoProjects/
│   ├── Components/
│   │   ├── Layout/            # Application shell and reconnect UI
│   │   ├── Pages/             # Routable pages, grouped by feature as they grow
│   │   ├── Shared/            # Reusable UI components
│   │   ├── App.razor          # HTML document, styles, and scripts
│   │   ├── Routes.razor       # Router and default layout
│   │   └── _Imports.razor     # Shared Razor imports
│   ├── Models/                # UI models, form models, and API response DTOs
│   ├── Services/              # Backend API clients and HTTP request handling
│   ├── Properties/            # Local launch profiles
│   ├── wwwroot/              # Public static assets and application CSS
│   ├── Program.cs            # Service registration and request pipeline
│   └── appsettings*.json      # Application configuration
└── README.md
```

Empty folders contain `.gitkeep` files so Git preserves the structure. Remove
these markers when adding code. Add feature folders such as `Pages/Projects/`
and `Pages/Supervisors/` as those features are implemented. Keep components used
only by one feature with that feature, and move reusable components to `Shared/`.
Register API clients and other services in `Program.cs`.

## Local development

Install the .NET 10 SDK, then run from the repository root:

```sh
dotnet restore frontend/uoProjects
dotnet run --project frontend/uoProjects --launch-profile http
```

Open `http://localhost:5075`. For automatic rebuilds while editing:

```sh
dotnet watch --project frontend/uoProjects --launch-profile http
```

Build the UI on its own or as part of the solution:

```sh
dotnet build frontend/uoProjects
dotnet build uoProjects.slnx
```

MudBlazor is registered in `Program.cs`; its providers live in `MainLayout.razor`
and its CSS and JavaScript are loaded in `App.razor`. See the official
[MudBlazor installation guide](https://mudblazor.com/getting-started/installation).

## Backend integration

The UI will consume the backend's `/api/v1` REST endpoints. API integration and
authentication are not implemented in this scaffold. Add API clients under
`Services/` when implementing features; keep HTTP calls out of page markup.
With Interactive Server rendering, those calls execute on the frontend server.
See the [backend guide](../backend/README.md) for configuration and endpoints.

## Branding and layout

`Theming/AppTheme.cs` defines the shared MudBlazor dark theme and typography.
`wwwroot/app.css` contains the original brand colour tokens, dark-mode semantic
colours, responsive layout, and shared native button/form classes. Use semantic
colours such as `--color-text-secondary` on dark surfaces; the original muted and
status text colours are retained for the brand reference. No Tailwind dependency
is required. MudBlazor components inherit their colours from the theme.

Work Sans is loaded through Google Fonts in `Components/App.razor`, with Arial as
an offline fallback. The logo is served from `wwwroot/uoProjects-logo.png`.
`Components/Layout/MainLayout.razor` owns the logo header, empty content container,
and providers. New pages automatically use this layout and dark theme.
