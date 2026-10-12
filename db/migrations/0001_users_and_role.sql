-- 0001: app identity
--
-- public.users extends the Supabase-managed auth.users with the one column the
-- rest of the schema depends on: role. Rows are created by handle_new_user() (0002).
--
-- | column | type             | null | notes                                      |
-- | ------ | ---------------- | ---- | ------------------------------------------ |
-- | id     | uuid             | no   | PK, FK -> auth.users(id) ON DELETE CASCADE |
-- | email  | text             | no   | copy of auth.users.email                   |
-- | role   | public.user_role | no   | student / supervisor / admin               |

create type public.user_role as enum ('student', 'supervisor', 'admin');

create table public.users (
  id uuid primary key references auth.users (id) on delete cascade,
  email text not null,
  role public.user_role not null
);
