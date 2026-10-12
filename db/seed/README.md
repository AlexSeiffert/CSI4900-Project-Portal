# db/seed

## database.mjs

Sets up the database: for each file in `db/migrations/` it checks whether what that migration creates already exists, and applies the file if not, so it's safe to run again. Run `mise x -- node db/seed/database.mjs` (needs `psql`; it uses `DATABASE_URL` if set, otherwise reads the connection string from Terraform).

## users.mjs

`users.mjs` creates, modifies and removes users on the live Supabase project for seeding admins and for making test users of any role. Only users: an admin can manage everything else through the app and RLS.

It talks to Supabase's Admin API with the service key, so `public.users` rows appear on their own via `handle_new_user()` (migration 0002).

### Usage

Run from the repo root with the pinned Node (`mise install` once, then `mise x -- node ...`

```sh
node db/seed/users.mjs create --email jane@uottawa.ca --role admin            # prints a random password once
node db/seed/users.mjs create --email test@gmail.com --role student --password 'something'
node db/seed/users.mjs modify --email test@gmail.com --role supervisor
node db/seed/users.mjs modify --id <uuid> --password 'new' --new-email other@gmail.com
node db/seed/users.mjs remove --email test@gmail.com          # dry run, shows who would be removed
node db/seed/users.mjs remove --email test@gmail.com --yes    # deletes the login and its public.users row
```

Roles: `student`, `supervisor`, `admin`.

### Config

Nothing to set up. The script reads `TF_VAR_supabase_access_token` from the repo's `.env`, then uses it to look up the project and its secret key from Supabase. (To skip the lookup, set `SUPABASE_PROJECT_REF` and `SUPABASE_SECRET_KEY` yourself.)

The secret key is a master key that ignores RLS. Never commit it or use it in a frontend.


### Things to know

- **The role that counts lives in `public.users.role`**, not signup metadata. `modify --role` updates that column; an already-issued token keeps the old role until the user signs in again.
- **`--new-email` changes the login email and the copy in `public.users`.** Changing the email any other way (e.g. in the Supabase dashboard) leaves `public.users.email` stale, because only the signup trigger writes it.
- **This runs against the live project** (there is no staging copy), so `remove` needs `--yes` to confirm deletion of a user record.
- Test emails must look real: Supabase rejects placeholder domains like `example.com`.
