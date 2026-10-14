-- 0007: projects posted by supervisors
--
-- | column        | type           | notes                                           |
-- | ------------- | -------------- | ----------------------------------------------- |
-- | id            | uuid           | PK                                              |
-- | supervisor_id | uuid           | FK -> supervisor_profiles(user_id), the owner   |
-- | title         | text           |                                                 |
-- | description   | text           |                                                 |
-- | external_url  | text           | optional                                        |
-- | status        | project_status | available / unavailable / archived              |
-- | posted_at     | timestamptz    | the "date posted" students see                  |
-- | updated_at    | timestamptz    | kept current by set_updated_at()                |
--
-- status doubles as the "availability" shown when browsing, so there is no separate column.
-- It can be changed on its own (the inline dropdown), without editing the rest of the project.
--
-- Everyone signed in can read every project. Only the owning supervisor can create, edit or
-- delete their own.

create type public.project_status as enum ('available', 'unavailable', 'archived');

create table public.projects (
  id uuid primary key default gen_random_uuid(),
  supervisor_id uuid not null references public.supervisor_profiles (user_id) on delete cascade,
  title text not null,
  description text not null,
  external_url text,
  status public.project_status not null default 'available',
  posted_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create index projects_supervisor_id_idx on public.projects (supervisor_id);

create trigger projects_set_updated_at
  before update on public.projects
  for each row execute function public.set_updated_at();

alter table public.projects enable row level security;

create policy projects_select_all
  on public.projects
  for select
  to authenticated
  using (true);

create policy projects_insert_own
  on public.projects
  for insert
  to authenticated
  with check (supervisor_id = auth.uid());

create policy projects_update_own
  on public.projects
  for update
  to authenticated
  using (supervisor_id = auth.uid())
  with check (supervisor_id = auth.uid());

create policy projects_delete_own
  on public.projects
  for delete
  to authenticated
  using (supervisor_id = auth.uid());

create policy projects_admin_full_access
  on public.projects
  for all
  to authenticated
  using (public.current_user_role() = 'admin')
  with check (public.current_user_role() = 'admin');
