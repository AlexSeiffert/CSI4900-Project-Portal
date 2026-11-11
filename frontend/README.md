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
│   ├── Services/              # Backend API services and HTTP request handling
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
Register API services in `Program.cs`.

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
mise exec -- dotnet watch --project frontend/uoProjects --launch-profile http-watch
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

The Projects page loads `GET /api/v1/projects` through the typed `ProjectsService`
in `Services/` and displays every returned project in a table with title,
description, and status columns. The table scrolls horizontally on narrow screens. It uses
the shared loading and empty states, with a retry button for failed requests.
Follow this pattern for other features: response models in `Models/`, typed HTTP
services in `Services/`, and registration in `Program.cs`.

Feature services inherit from `HttpBaseService`, which provides `GetAsync<T>` for
JSON responses and error handling. Keep endpoint paths in the feature
service, as in `ProjectsService.GetProjectsAsync`. Register additional services with
`.AddTypedClient<YourService>()` on the named `"Api"` HTTP client in `Program.cs`
to reuse its base address, 30-second request timeout, and development authentication configuration.

The base service reads the API's Problem Details into `ServiceException`, retaining
the HTTP status, validation errors, and trace ID. It uses HTTP status codes directly
rather than duplicating the backend's error categories. Missing or malformed error
bodies get a safe fallback message; connectivity failures, timeouts, and invalid
successful responses are wrapped in the same exception. Caller cancellation
propagates unchanged. The Projects page cancels pending requests when disposed.

Feature services provide an action-specific failure message to `GetAsync<T>`.
The base service reports failures through `NotificationService.Error(message,
exception)` and returns `null`; a successful empty list stays an empty list.
The notification service logs once and displays the action, safe error details,
validation feedback, and the API trace ID. Notifications appear at the bottom
right and dismiss after five seconds. `Program.cs` owns the shared snackbar
configuration; operation keys prevent duplicate snackbars.

Following the Telerik grid demo's service and `LoadData` pattern, the Projects
page binds data returned by `ProjectsService`. Its markup, code-behind, and styles live together in `Components/Pages/Projects/`,
with display state and `LoadDataAsync` in `Projects.razor.cs`. HTTP handling and
failure notifications belong to the services. Loading begins in `OnInitializedAsync`,
so the initial server-rendered page completes even without an interactive circuit.
`PersistentState` preserves the result (including failure) when the circuit starts,
avoiding a duplicate request. Restored failures show a notification once interactive.
The page handles only lifecycle cancellation and uses `ErrorState` for a short
inline message and retry action. A `null` result shows the failure state; an empty
list shows the empty state.

Run the frontend error-handling tests with:

```sh
dotnet test frontend/tests/uoProjects.Tests
```

`Api:BaseUrl` in `appsettings.json` points to `https://localhost:7049/` by default.
Override it with `Api__BaseUrl` when the backend runs elsewhere; include a trailing
slash. With Interactive Server rendering, API calls execute on the frontend
server, so browser CORS configuration is not required.

Login is not implemented yet and the API requires authentication. For local
development, add `Api__DevUserId=<existing Supabase user UID>` to the root `.env`.
The frontend's VS Code launch command uses `mise exec` to load this file, just
like the backend launch configuration. Restart the frontend after changing `.env`.
Alternatively, set the `Api__DevUserId` environment variable before starting it.
The service sends the API's existing `X-Dev-User-Id` header
only when the frontend runs in Development. Both apps must run in Development
for this shortcut. Leave the checked-in value empty; do not commit personal
configuration. Without a valid user ID the page shows an authentication error.
The API's VS Code launch uses its `https-supabase` profile's fixed ports
(`7049` for HTTPS and `5049` for HTTP), matching the frontend's default API address.
See the [backend guide](../backend/README.md) for running the API and finding user IDs.
