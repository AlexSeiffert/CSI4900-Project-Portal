# Frontend (uoProjects)

A .NET 10 Blazor WebAssembly app in `frontend/uoProjects/`, using MudBlazor for components and Tailwind for styling. It calls the backend API directly through typed services in `Services/`. Shared HTTP handling supplies timeouts, JSON serialization, and problem-details notifications.

Routable views live in feature folders under `Pages/`. Admin components are grouped by feature under `Admin/`, and reusable components are grouped under `Components/`. Pages and components with substantial C# logic keep it in an adjacent `.razor.cs` file. Scoped `.razor.css` files live beside their markup. Tailwind input and the MudBlazor preparation script are in `Styles/`, and the MudBlazor theme is in `Theming/`. Brand colours, typography and status meanings are in [BRAND.md](BRAND.md).

## Pages

| Route | Page | Access |
| --- | --- | --- |
| `/`, `/login` | Sign in | Anyone |
| `/student-home` | Student home | Student only |
| `/supervisor-home` | Supervisor home | Supervisor only |
| `/admin-home` (and `/admin-home/{Tab}/{ItemId}`) | Admin dashboard | Admin only |
| `/projects`, `/supervisors`, `/user` | Browse and profile shells | Students and supervisors |
| `/admin-preview` | Read-only sample dashboard | Development only |

`RoleGate` (in `Auth/`) redirects signed-out users to `/login` and users with the wrong role to their own home.

The admin Projects list loads 25 projects at a time. Search, filters, and sorting apply across all projects. Exports fetch the full selected result set in pages when requested.

## Local development

Open the repository root in VS Code. In Run and Debug, select **uoProjects API** and press **F5** to start the API debugger and open Swagger. Then select **uoProjects Frontend** and press **F5** to start the WASM app at `http://localhost:5270`.

From a terminal:

```sh
mise exec -- dotnet watch --project frontend/uoProjects --launch-profile http-watch
npm run css:watch --prefix frontend/uoProjects
```

Tailwind only compiles during a build, so keep `css:watch` running to see style edits without a rebuild. The build installs npm packages when `node_modules` is missing, so Node is required. Set `SkipTailwind=true` to skip the Tailwind step.

Sign-in needs the Supabase values in the root `.env` (see the [backend guide](../backend/README.md)).


Only public settings belong in `wwwroot`. Set `Api:BaseUrl`, `Supabase:AuthUrl` (including `/auth/v1`), and `Supabase:PublishableKey` in the static host configuration. For local development, `mise exec` loads `Auth__Authority` and `SUPABASE_PUBLISHABLE_KEY` from the root `.env` and the build writes those two public settings to the development browser configuration. Configure the API to allow the frontend origin and the static host to serve `index.html` for application routes.

Run the frontend and API test projects with `dotnet test uoProjects.slnx` from the repository root.
