# db/tests

pgTAP tests for the database, written as plain SQL. Each file runs inside a transaction that rolls back, so nothing is ever left behind.

| File | Covers |
| --- | --- |
| `01_signup_and_insert.sql` | signup creates the right row; bad roles, duplicates, nulls and bad ids are rejected |
| `02_update_and_delete.sql` | who can update or delete what, and what cascades |
| `03_read_access.sql` | what each kind of caller can see |
| `04_token_hook.sql` | what the login hook puts on the token, and who may call it |
| `05_structure.sql` | tables, columns, keys, triggers, policies and grants |

## How they run

On every PR that touches `db/**`, GitHub Actions (`.github/workflows/db-tests.yml`) starts a throwaway Supabase database, applies `db/migrations/*.sql` in order, and runs `supabase test db`. A failing test fails the check. They never touch the live project.

## Not covered here

The real sign-in flow (a token actually issued with `user_role`) needs Supabase Auth running against the live project, so it isn't part of these tests. To check it by hand, see "Testing the identity migrations without a frontend" on the wiki's Deployment page.
