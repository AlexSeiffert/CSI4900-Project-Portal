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
| `10_inquiries.sql` | who can send, see and change inquiries; what survives a deletion |
| `11_attachments.sql` | attachment rows, the storage bucket, and who can touch the files |
| `12_saved_projects.sql` | a student's saved projects |
| `13_structure_domain.sql` | tables, keys, triggers, policies and grants for migrations 0005-0011 |

## How they run

On every PR that touches `db/**`, GitHub Actions (`.github/workflows/database.yml`) starts a throwaway Supabase database, applies `db/migrations/*.sql` in order, and runs `supabase test db`. A failing test fails the check. They never touch the live project.

## Not covered here

The real sign-in flow (a token actually issued with `user_role`) needs Supabase Auth running against the live project, so it isn't part of these tests.
