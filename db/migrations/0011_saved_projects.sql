-- 0011: a student's saved projects (their wishlist)
--
-- | column     | type        | notes                              |
-- | ---------- | ----------- | ---------------------------------- |
-- | student_id | uuid        | FK -> student_profiles(user_id)    |
-- | project_id | uuid        | FK -> projects(id)                 |
-- | saved_at   | timestamptz | when it was saved                  |
--
-- The primary key is (student_id, project_id), so a project can only be saved once.
-- Saving is not the same as applying: "projects I've already inquired about" is a query
-- over inquiries (0009), not a table here.
--
-- A student can see, add and remove only their own saved projects.

create table public.student_saved_projects (
  student_id uuid not null references public.student_profiles (user_id) on delete cascade,
  project_id uuid not null references public.projects (id) on delete cascade,
  saved_at timestamptz not null default now(),
  primary key (student_id, project_id)
);

create index student_saved_projects_project_id_idx on public.student_saved_projects (project_id);

alter table public.student_saved_projects enable row level security;

create policy student_saved_projects_select_own
  on public.student_saved_projects
  for select
  to authenticated
  using (student_id = auth.uid());

create policy student_saved_projects_insert_own
  on public.student_saved_projects
  for insert
  to authenticated
  with check (student_id = auth.uid());

create policy student_saved_projects_delete_own
  on public.student_saved_projects
  for delete
  to authenticated
  using (student_id = auth.uid());

create policy student_saved_projects_admin_full_access
  on public.student_saved_projects
  for all
  to authenticated
  using (public.current_user_role() = 'admin')
  with check (public.current_user_role() = 'admin');
