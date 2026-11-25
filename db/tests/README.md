# db/tests

> **These tests run in CI, not against a live database.** On every PR that touches `db/**`, the GitHub Actions workflow `.github/workflows/database.yml` starts a throwaway Supabase stack in Docker containers, runs the tests against it, and deletes it afterwards. The live project is never touched.


| File | Covers |
| --- | --- |
| `01_signup_and_insert.sql` | signup creates the right row; bad roles, duplicates, nulls and bad ids are rejected |
| `02_update_and_delete.sql` | who can update or delete what, and what cascades |
| `03_read_access.sql` | what each kind of caller can see |
| `04_token_hook.sql` | what the login hook puts on the token, and who may call it |
| `05_structure.sql` | tables, columns, keys, triggers, policies and grants |
| `06_profiles.sql` | profiles are created automatically; who can read and edit them |
| `07_topics.sql` | topics are reused ignoring case; nobody writes to the table directly |
| `08_projects.sql` | who can create, edit, change the status of and delete projects |
| `09_topic_links.sql` | project tags, supervisor interests and student interests |
| `10_inquiries.sql` | group requests: adding friends by email, the 3-member limit, who can see them, every status change (only a student's confirmation produces `matched`), and what survives a deletion |
| `11_attachments.sql` | each member's attachment rows and files (`resume` / `transcript`), the storage bucket, who can touch them, and that a member who leaves takes their files with them |
| `12_saved_projects.sql` | a student's saved projects |
| `13_structure_domain.sql` | tables, keys, triggers, policies and grants for migrations 0005-0011 |

## Run them locally

With Docker running, start a throwaway stack in any empty folder outside the repo, apply the migrations, and run the tests there:

```sh
supabase init && supabase start
for f in <repo>/db/migrations/*.sql; do docker exec -i supabase_db_<folder> psql -U postgres -v ON_ERROR_STOP=1 -q < "$f"; done
mkdir -p supabase/tests && cp <repo>/db/tests/*.sql supabase/tests/ && supabase test db
```

## How they run

On every PR that touches `db/**`, GitHub Actions (`.github/workflows/database.yml`) starts a throwaway Supabase database, applies `db/migrations/*.sql` in order, and runs `supabase test db`. A failing test fails the check. They never touch the live project.

## Not covered here

The real sign-in flow (a token actually issued with `user_role`) needs Supabase Auth running against the live project, so it isn't part of these tests.
