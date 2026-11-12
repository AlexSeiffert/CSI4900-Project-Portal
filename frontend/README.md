# Frontend (uoProjects)

A .NET 10 Blazor WebAssembly app in `frontend/uoProjects/`, using MudBlazor for components and Tailwind for styling. It calls the backend API directly.

Code is grouped by feature (`Admin/`, `Auth/`, `Pages/`, `Components/`, `Layout/`). Tailwind input and the MudBlazor preparation script are in `Styles/`, and the MudBlazor theme is in `Theming/`. Brand colours, typography and status meanings are in [BRAND.md](BRAND.md).

## Pages

| Route | Page | Access |
| --- | --- | --- |
| `/`, `/login` | Sign in | Anyone |
| `/student-home` | Student home | Student only |
| `/supervisor-home` | Supervisor home | Supervisor only |
| `/admin-home` (and `/admin-home/{Tab}/{ItemId}`) | Admin dashboard | Admin only |
| `/projects`, `/supervisors`, `/user` | Browse pages | Not gated yet |

`RoleGate` (in `Auth/`) redirects signed-out users to `/login` and users with the wrong role to their own home.

## Local development

Open the repository root in VS Code and run **uoProjects (Full Stack)** (API, frontend and CSS watcher), or just **uoProjects Frontend**, then press **F5**. The frontend serves at `http://localhost:5270`, which the backend's CORS policy allows.

From a terminal:

```sh
dotnet watch --project frontend/uoProjects --launch-profile http-watch
npm run css:watch --prefix frontend/uoProjects
```

Tailwind only compiles during a build, so keep `css:watch` running to see style edits without a rebuild. The build installs npm packages when `node_modules` is missing, so Node is required. Set `SkipTailwind=true` to skip the Tailwind step.

Sign-in needs the Supabase values in the root `.env` (see the [backend guide](../backend/README.md)).
