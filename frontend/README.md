# Frontend (uoProjects)

A .NET 10 Blazor WebAssembly app in `frontend/uoProjects/`, using MudBlazor for components and Tailwind for styling. It calls the backend API directly through typed services in `Services/`. Shared HTTP handling supplies timeouts, JSON serialization, and problem-details notifications.

Routable views live in feature folders under `Pages/`. Admin components are grouped by feature under `Admin/`, and reusable components are grouped under `Components/`. Pages and components with substantial C# logic keep it in an adjacent `.razor.cs` file. Scoped `.razor.css` files live beside their markup. Tailwind input and the MudBlazor preparation script are in `Styles/`, and the MudBlazor theme is in `Theming/`. Brand colours, typography and status meanings are in [BRAND.md](BRAND.md).

## Pages

| Route | Page | Access |
| --- | --- | --- |
| `/`, `/login` | Sign in | Anyone |
| `/student-home` | The student's requests | Student only |
| `/student-home/requests/new` (optional `?projectId=`) | Start a request: pick a project, write a message (2000 characters max) | Student only |
| `/student-home/requests/{id}` | One request: add friends by email, upload a résumé and transcript, send it, answer an offer | Student only |
| `/supervisor-home` | Inquiries sent to the supervisor | Supervisor only |
| `/supervisor-home/inquiries/{id}` | One inquiry: message, students, every student's files, offer a match or close | Supervisor only |
| `/admin-home` (and `/admin-home/{Tab}/{ItemId}`) | Admin dashboard | Admin only |
| `/projects`, `/supervisors`, `/user` | Browse and profile shells | Students and supervisors |
| `/admin-preview` | Read-only sample dashboard | Development only |

`RoleGate` (in `Auth/`) redirects signed-out users to `/login?returnUrl=…`, which brings them back after signing in, and users with the wrong role to their own home.

## Requests and inquiries

The pages in `Inquiries/` cover group requests (see "Inquiries" in the [backend guide](../backend/README.md) for the statuses).

- **Send** opens Outlook on the web in a new tab (`Inquiries/OutlookLink.cs`): To the supervisor's contact email, CC the other members, and a body of the message plus one link, `/supervisor-home/inquiries/{id}`. That link is the same page supervisors reach from their list, and only that supervisor can open it. When the student comes back, "Did you send it?" marks the request as sent; "No" leaves it a draft.
- Each member uploads their own PDFs straight to Supabase Storage through a signed URL from the API, so uploads and view links need a real Supabase sign-in.
- On an offer, each member accepts or declines. Accepting while holding other offers asks first, then drops the student from those.

The admin Projects list loads 25 projects at a time. Search, filters, and sorting apply across all projects. Exports fetch the full selected result set in pages when requested.

## Local development

Open the repository root in VS Code, select **uoProjects (Full Stack)** in Run and Debug, and press **F5**. It starts the API, the frontend at `http://localhost:5270`, and the Tailwind watcher. **uoProjects Frontend** and **uoProjects CSS** run those parts alone, and **uoProjects API (Debugger)** runs the API under the debugger and opens Swagger.

From a terminal:

```sh
mise exec -- dotnet watch --project frontend/uoProjects --launch-profile http-watch
npm run css:watch --prefix frontend/uoProjects
```

Tailwind only compiles during a build, so keep `css:watch` running to see style edits without a rebuild. The build installs npm packages when `node_modules` is missing, so Node is required. Set `SkipTailwind=true` to skip the Tailwind step.

Sign-in needs the Supabase values in the root `.env` (see the [backend guide](../backend/README.md)).


Only public settings belong in `wwwroot`. Set `Api:BaseUrl`, `Supabase:AuthUrl` (including `/auth/v1`), and `Supabase:PublishableKey` in the static host configuration. For local development, `mise exec` loads `Auth__Authority` and `SUPABASE_PUBLISHABLE_KEY` from the root `.env` and the build writes those two public settings to the development browser configuration. Configure the API to allow the frontend origin and the static host to serve `index.html` for application routes.

Run the frontend and API test projects with `dotnet test uoProjects.slnx` from the repository root.
