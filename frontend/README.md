# Frontend

The UI is a .NET 10 Blazor Web App using MudBlazor and global Interactive
Server rendering. The frontend runs as a separate application from the backend API.

## Folder structure

```text
frontend/
├── uoProjects/
│   ├── Components/
│   │   ├── Layout/            # Application shell
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

Install the .NET 10 SDK and the workspace's recommended C# and C# Dev Kit
extensions. Open the repository root in VS Code, select **uoProjects Frontend
(Hot Reload)** in **Run and Debug**, and press **F5**.

This is the single frontend launch option. It runs `dotnet watch` using the
`http-watch` profile and opens `http://localhost:5076`. Saved changes are hot
reloaded; edits that require a restart restart the app automatically. Stop it
with **Ctrl+C** in the watcher terminal. Run only one frontend instance at a time;
if port 5076 is occupied, stop the earlier instance before starting another.
This option runs the watcher without attaching the C# debugger.

The same setup from a terminal:

```sh
dotnet restore frontend/uoProjects
dotnet watch --project frontend/uoProjects --launch-profile http-watch
```

**uoProjects API (Supabase)** is the backend launch option and reads the root
`.env`; see the backend guide for its required configuration.

If Razor components show `RZ10012` warnings despite a successful build, check
VS Code's status bar. If it says **No Solution**, click it and open
`uoProjects.slnx`. The C# project must be loaded for Razor IntelliSense to resolve
framework and MudBlazor components.
Install the workspace's recommended C# and C# Dev Kit extensions. Wait for solution
loading to finish; if the editor still treats files as miscellaneous files,
run **Developer: Reload Window** after opening the solution. A successful CLI
build alone does not confirm that the editor has loaded the project.

Build the UI on its own or as part of the solution:

```sh
dotnet build frontend/uoProjects
dotnet build uoProjects.slnx
```

MudBlazor is registered in `Program.cs`; its providers live in `MainLayout.razor`
and its CSS and JavaScript are loaded in `App.razor`. See the official
[MudBlazor installation guide](https://mudblazor.com/getting-started/installation).

## Shared content patterns

Use the components in `Components/Shared/` when a feature page first needs these
states. The base pages remain header-only. Imports are provided by
`Components/_Imports.razor`.

```razor
@if (isLoading)
{
    <LoadingState Message="Loading projects…" />
}
else if (projects.Count == 0)
{
    <EmptyState Title="No projects found"
                Description="Try changing your search or filters.">
        <MudButton OnClick="ClearFilters" Variant="Variant.Outlined">Clear filters</MudButton>
    </EmptyState>
}
else
{
    @* Render the feature's existing list or table here. *@
}
```

`LoadingState` announces its message to assistive technology. `EmptyState` accepts
a required title, optional description and icon, and optional child actions. Use
an empty state only after a successful request with no results; request failures
should have their own retry/error handling.

`FormValidationSummary` uses Blazor's built-in validation messages and belongs
inside an `EditForm`. Add a validator and field messages as usual:

```razor
<EditForm Model="profile" OnValidSubmit="SaveProfile" FormName="profile">
    <DataAnnotationsValidator />
    <FormValidationSummary />
    <label class="label" for="display-name">Display name</label>
    <InputText id="display-name" class="field" @bind-Value="profile.DisplayName"
               aria-describedby="display-name-error" />
    <div id="display-name-error">
        <ValidationMessage For="@(() => profile.DisplayName)" />
    </div>
    <MudButton ButtonType="ButtonType.Submit" Variant="Variant.Filled" Color="Color.Primary">Save</MudButton>
</EditForm>
```

These snippets assume feature-owned models, state, and handlers. Add data
annotations to the form model; the summary adds presentation and announcements,
without replacing validation or submission logic. For `MudForm`, use MudBlazor's
validation API and field errors rather than nesting it inside `EditForm`.

## Backend integration

The UI will consume the backend's `/api/v1` REST endpoints. API integration and
authentication are not implemented in this scaffold. Add API clients under
`Services/` when implementing features; keep HTTP calls out of page markup.
With Interactive Server rendering, those calls execute on the frontend server.
See the [backend guide](../backend/README.md) for configuration and endpoints.
