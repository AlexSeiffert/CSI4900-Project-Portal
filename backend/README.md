#  API

ASP.NET Core Web API (.NET 10). This guide gets you from a fresh machine to calling every endpoint in Swagger.

## 1. Setup (once)

Tools are pinned in [.tool-versions](../.tool-versions) and installed with [mise](https://mise.jdx.dev).
These commands are for **macOS** (Homebrew, zsh). On Linux or WSL, install mise with `curl https://mise.run | sh`
and use `~/.bashrc` with `mise activate bash`; see the [mise install docs](https://mise.jdx.dev/installing-mise.html) for other systems.

```sh
brew install mise
echo 'eval "$(mise activate zsh)"' >> ~/.zshrc && source ~/.zshrc
mise install            # run in the repo root
dotnet --version        # 10.0.402
```

Then create `.env` in the repo root (gitignored; ask a teammate for the values). mise loads it whenever you're in the repo.

| Variable | What it is |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | `Host=<pooler host>;Port=5432;Database=postgres;Username=postgres.<ref>;Password=<db password>;SSL Mode=Require` (port **5432**, not 6543) |
| `Auth__Authority` | `https://<ref>.supabase.co/auth/v1` |
| `SUPABASE_PUBLISHABLE_KEY` | `sb_publishable_...`, for getting login tokens |
| `TF_VAR_supabase_access_token`, `TF_VAR_db_password` | used by Terraform and the [db/seed](../db/seed) scripts |

## 2. Run

From the repo root. The API is at **https://localhost:7049/swagger**.

```sh
dotnet run --project backend/API --launch-profile https-supabase   # real Supabase database
dotnet run --project backend/API                                   # fake in-memory database, no .env needed
```

## 3. Log in to Swagger

Click **Authorize** and fill in the details of the role you're testing, in one of two ways:

- **Development**: paste the user's id. The API treats you as that user, no password needed.
- **Bearer**: paste a real login token, which is what the frontend will send. Get one with the command below
  (it expires after an hour). If both boxes are filled in, the token is used and the Development id is ignored,
  so click **Logout** on Bearer before switching to a Development id:

```sh
curl -s "$Auth__Authority/token?grant_type=password" -H "apikey: $SUPABASE_PUBLISHABLE_KEY" \
  -H "Content-Type: application/json" -d '{"email":"<email>","password":"<password>"}' | jq -r .access_token
```

Test accounts on the real database (passwords from a teammate, or set one with
`mise x -- node db/seed/users.mjs modify --email <email> --password '<new>'`):

Some example users we created are:

| Role | Name | Email |
| --- | --- | --- |
| student | Alex Morgan | alex.morgan@test-student.dev |
| student | Maya Singh | maya.singh@test-student.dev |
| supervisor | Dr. Marcus Ahmed | marcus.ahmed@test-supervisor.dev |
| supervisor | Dr. Elena Chen | elena.chen@test-supervisor.dev |
| admin | Sam Taylor | sam.taylor@test-admin.dev |

To find a user's id, check Supabase: **Authentication → Users** in the dashboard, then copy the **UID** next to their email.

## 4. Try the endpoints

To call an endpoint, click it, click **Try it out**, fill in the fields and click **Execute**.

- Swagger pre-fills ids like `3fa85f64-5717-4562-b3fc-2c963f66afa6`. These are placeholders, so replace them with real ids (see section 3).
- The same request can return different results for different users. For example, a student only sees their own inquiries.

### Walkthrough

| Log in as | Try | Expect |
| --- | --- | --- |
| anyone | `GET /topics`, `GET /supervisors`, `GET /projects` | lists |
| anyone | `GET /projects/statuses/available`, `GET /projects/supervisors/<Elena's id>` | filtered lists |
| Marcus | `POST /projects` (body below) | **201**, you own it |
| Marcus | `PUT` then `DELETE /projects/<your new project's id>` | **200**, then **204** |
| Marcus | `PUT /supervisors/<Marcus's id>` with your profile fields | **200** |
| Alex | `GET /students`, `GET /students/<Alex's id>` | just yourself |
| Alex | `GET /inquiries`, `GET /inquiries/<one of Alex's inquiry ids>` | your inquiries only |
| Alex | `POST /inquiries` (body below) | **201**, status `pending` |
| Alex | `PUT /students/<Alex's id>/saved-projects/<any project id>` | **204**, then it shows in `GET .../saved-projects` |
| Sam | `GET /students`, `GET /inquiries` | everything |

### Request bodies

For `POST /projects` (paste it over Swagger's example):

```json
{ "title": "Test project", "description": "Testing", "externalUrl": null, "status": "available", "topicIds": [] }
```

For `POST /inquiries`. The project must belong to the supervisor you're contacting:

```json
{ "supervisorId": "<Elena's id>", "projectId": "<one of Elena's project ids>", "message": "Hi!" }
```

### Permission checks

As Marcus, editing Elena's project or profile gives **403**. As Alex, `GET /students/<Maya's id>` gives
**404**, and saving into Maya's list gives **403**. As Sam, editing anything works.

### Status codes

| Code | Meaning |
| --- | --- |
| 200 / 201 / 204 | worked (201 created, 204 nothing to return) |
| 400 | bad input, or a supervisor, project or student id that doesn't exist |
| 401 | not logged in |
| 403 | not allowed by the permission rules |
| 404 | doesn't exist, or you can't see it |
| 409 | a topic id that doesn't exist, or a duplicate |
| 500 | server error; find the `traceId` in the API's terminal |

### Responses and updates

- **Lists only have the basic fields.** `GET /projects` gives each project's title, status and so on.
  `GET /projects/<id>` also includes its supervisor, topics and inquiries.
- **Related records are kept short.** The supervisor inside a project doesn't list all their other projects.
  Call `GET /supervisors/<id>` for that.
- **`topicIds` replaces, it doesn't add.** If a project has topics A and B and you send `"topicIds": ["<C>"]`,
  it ends up with only C.

### All endpoints

Every path starts with `/api/v1`.

| Method | Path | What it does | Success |
| --- | --- | --- | --- |
| **Topics** | | | |
| GET | `/topics` | List all topics | 200 |
| **Projects** | | | |
| GET | `/projects` | List projects, newest first | 200 |
| GET | `/projects/<id>` | One project with its supervisor, topics and inquiries | 200 |
| GET | `/projects/supervisors/<supervisor id>` | A supervisor's projects | 200 |
| GET | `/projects/topics/<topic id>` | Projects with a topic | 200 |
| GET | `/projects/statuses/<status>` | Projects that are `available`, `unavailable` or `archived` | 200 |
| POST | `/projects` | Create a project (supervisors; you become the owner) | 201 |
| PUT | `/projects/<id>` | Edit a project (owner or admin) | 200 |
| DELETE | `/projects/<id>` | Delete a project (owner or admin) | 204 |
| **Inquiries** | | | |
| GET | `/inquiries` | List the inquiries you're part of (admins: all) | 200 |
| GET | `/inquiries/<id>` | One inquiry with its student, supervisor and project | 200 |
| GET | `/inquiries/students/<student id>` | A student's inquiries | 200 |
| GET | `/inquiries/supervisors/<supervisor id>` | Inquiries sent to a supervisor | 200 |
| GET | `/inquiries/projects/<project id>` | Inquiries about a project | 200 |
| POST | `/inquiries` | Send an inquiry (students; you're the sender) | 201 |
| **Students** | | | |
| GET | `/students` | List student profiles you can see (students: only yourself) | 200 |
| GET | `/students/<id>` | One profile with topics, inquiries and saved projects | 200 |
| PUT | `/students/<id>` | Edit a profile (that student or admin) | 200 |
| **Saved projects** | | | |
| GET | `/students/<student id>/saved-projects` | A student's saved projects | 200 |
| PUT | `/students/<student id>/saved-projects/<project id>` | Save a project (saving twice is fine) | 204 |
| DELETE | `/students/<student id>/saved-projects/<project id>` | Unsave a project | 204 |
| **Supervisors** | | | |
| GET | `/supervisors` | List all supervisors | 200 |
| GET | `/supervisors/<id>` | One profile with topics, projects and inquiries | 200 |
| PUT | `/supervisors/<id>` | Edit a profile (that supervisor or admin) | 200 |

## 5. Tests

```sh
dotnet test uoProjects.slnx
```

They use the fake database, need no `.env`, and run on every pull request. Database permission rules are tested in [db/tests](../db/tests).

## 6. Troubleshooting

| Problem | Fix |
| --- | --- |
| `dotnet: command not found` | mise isn't activated (step 1). `mise activate` alone only prints a script |
| `permission denied: ~/.zshrc` | `sudo chown $USER ~/.zshrc`, then redo step 1 |
| Every request is 401 | Log in with **Authorize** |
| Switched user, same results | Logout of the Bearer box |
| 400 with ids you didn't change | Replace Swagger's example ids with real ones |
| "Legacy API keys are disabled" | Use the publishable key, not the old `anon` key |
| Old behaviour after a code change | Restart the API |
| `$Auth__Authority` is empty | `cd` into the repo again so mise loads `.env` |

## 7. Code layout

- Requests go `Controller → Service → Repository` ([Controllers](API/Controllers), [Services](API/Services),
  [Repositories](API/Repositories)); response shapes are in [DTOs](API/DTOs).
- Errors become the codes above in [ApiExceptionHandler](API/Middleware/ApiExceptionHandler.cs).
- Logins are Supabase tokens; in Development the `X-Dev-User-Id` header also works ([Auth](API/Auth)). The caller's
  id is passed to Postgres, whose row level security rules in [db/migrations](../db/migrations) decide what each user
  can see and change. Roles live in `public.users.role`.
- The schema is owned by the SQL in [db/migrations](../db/migrations), not Entity Framework migrations.
