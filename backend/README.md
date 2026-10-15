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

## Logging in

Every request needs a Supabase login token (JWT) in an `Authorization: Bearer <token>` header.
The API reads who you are from the token. It never trusts an id in the request body.

The API passes your token's identity to the database, so the row level security rules in
[db/migrations](../db/migrations) decide what you can see and change.

## Try it

### Quick (no Supabase needed)

In Development the API uses a fake in-memory database filled with sample data, so you can
try it without an account.

```sh
dotnet run --project backend/API
```

- `dotnet run` builds and starts the API.
- `--project backend/API` says which project to start. Run it from the repo root.
- It listens on `https://localhost:7049`. Plain `http://localhost:5049` only redirects there, and `curl` prints nothing for a redirect.
- `-k` in the `curl` examples below lets `curl` accept the local development certificate.

Normally you log in and send a token. Here you pretend to be a sample user instead.

```sh
curl -k -H "X-Dev-User-Id: 11111111-1111-1111-1111-111111111111" https://localhost:7049/api/v1/projects
curl -k -H "X-Dev-User-Id: 22222222-2222-2222-2222-222222222222" https://localhost:7049/api/v1/projects
```

- `curl` sends an HTTP request. These two are `GET` requests (the default).
- `-H "X-Dev-User-Id: ..."` adds a header saying who you are. It only works in Development.
- `11111111-...-111` is the sample supervisor Dr. Elena Chen. `22222222-...-222` is the sample student Alex Morgan.
- `/api/v1/projects` is the endpoint that lists projects.

Other sample users have the same id with a different last digit: supervisors `...111` to `...114`,
students `...222` to `...226`. Names are in [DevelopmentData.cs](API/Models/Data/DevelopmentData.cs).
These fixed ids exist only for the fake database. With real Supabase, ids are created at signup.

The fake database ignores the permission rules (row level security), so it can't show who may
see what. Its data resets when the API restarts.

### With real Supabase

**1. Create a user.** In the dashboard go to Authentication → Users and add one with
**Auto Confirm User** ticked. In user metadata set `{"role": "student"}` or `{"role": "supervisor"}`.

**2. Start the API against Supabase.**

```sh
export ASPNETCORE_ENVIRONMENT=Development
export Database__Provider=Postgres Database__SeedDevelopmentData=false
export Auth__Authority=https://<ref>.supabase.co/auth/v1
export ConnectionStrings__DefaultConnection="Host=<pooler host>;Port=5432;Database=postgres;Username=postgres.<ref>;Password=<db password>;SSL Mode=Require"
dotnet run --project backend/API
```

- `export` sets a setting for this terminal only.
- `ASPNETCORE_ENVIRONMENT=Development` turns on development mode.
- `Database__Provider=Postgres` uses the real database instead of the fake one.
- `Database__SeedDevelopmentData=false` stops the sample data from being added.
- `Auth__Authority` is your project's login address. The API uses it to check that tokens are real.
- `ConnectionStrings__DefaultConnection` is how the API connects to the database.
  - `<ref>` is the project id, `<pooler host>` is the host from Supabase's Connect page, `<db password>` is the database password.
  - Use port **5432**, not 6543. The 6543 port drops the user identity the API sets on each connection.
- `dotnet run ...` starts the API.

**3. Log in and call the API.**

```sh
TOKEN=$(curl -s "https://<ref>.supabase.co/auth/v1/token?grant_type=password" \
  -H "apikey: <publishable key>" -H "Content-Type: application/json" \
  -d '{"email":"<email>","password":"<password>"}' | jq -r .access_token)

curl -k -H "Authorization: Bearer $TOKEN" https://localhost:7049/api/v1/projects
```

- `TOKEN=$(...)` runs the command inside and saves its result in a variable called `TOKEN`.
- The first `curl` logs in at Supabase. `-s` hides the progress output.
- `-H "apikey: ..."` identifies your project. It is the publishable key (Project Settings → API Keys). It is safe to share, unlike the secret key.
- `-d '{...}'` is the email and password of the user you created.
- `jq -r .access_token` picks the token out of the response.
- The second `curl` calls the API. `Authorization: Bearer $TOKEN` is how you prove who you are.
- Tokens expire after an hour. Run the login again.

### Results

| You get | Meaning |
| --- | --- |
| 401 | missing or bad token |
| 403 | not allowed (e.g. editing someone else's project) |
| 400 | bad data (e.g. an id that doesn't exist) |
