-- 0009: inquiries, a student reaching out to a supervisor
--
-- One row per inquiry. The conversation itself happens over email, not in the app.
--
-- | column        | type           | notes                                                  |
-- | ------------- | -------------- | ------------------------------------------------------ |
-- | id            | uuid           | PK                                                     |
-- | student_id    | uuid           | FK -> student_profiles(user_id)                        |
-- | supervisor_id | uuid           | FK -> supervisor_profiles(user_id)                     |
-- | project_id    | uuid           | optional (null = a general inquiry); FK -> projects     |
-- | status        | inquiry_status | pending / viewed / responded / closed                  |
-- | message       | text           | optional                                               |
-- | created_at    | timestamptz    |                                                        |
-- | updated_at    | timestamptz    | kept current by set_updated_at()                       |
--
-- If the project is later deleted, the inquiry stays and project_id becomes null.
--
-- Only the student and the supervisor involved can see an inquiry. A student creates one
-- (always starting as pending, and only for a project that belongs to that supervisor).
-- After that only the status can change, and either party can change it.

create type public.inquiry_status as enum ('pending', 'viewed', 'responded', 'closed');

create table public.inquiries (
  id uuid primary key default gen_random_uuid(),
  student_id uuid not null references public.student_profiles (user_id) on delete cascade,
  supervisor_id uuid not null references public.supervisor_profiles (user_id) on delete cascade,
  project_id uuid references public.projects (id) on delete set null,
  status public.inquiry_status not null default 'pending',
  message text,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create index inquiries_student_id_idx on public.inquiries (student_id);
create index inquiries_supervisor_id_idx on public.inquiries (supervisor_id);
create index inquiries_project_id_idx on public.inquiries (project_id);

create trigger inquiries_set_updated_at
  before update on public.inquiries
  for each row execute function public.set_updated_at();

-- Only an admin may change anything other than status. project_id may still be cleared,
-- which is what happens when the project is deleted.
create or replace function public.limit_inquiry_updates()
returns trigger
language plpgsql
set search_path = public
as $$
begin
  if auth.uid() is not null
     and public.current_user_role() is distinct from 'admin'
     and (
       (new.student_id, new.supervisor_id, new.message, new.created_at)
         is distinct from (old.student_id, old.supervisor_id, old.message, old.created_at)
       or (new.project_id is not null and new.project_id is distinct from old.project_id)
     ) then
    raise exception 'only the status of an inquiry can be changed' using errcode = '42501';
  end if;

  return new;
end;
$$;

create trigger inquiries_limit_updates
  before update on public.inquiries
  for each row execute function public.limit_inquiry_updates();

alter table public.inquiries enable row level security;

create policy inquiries_select_involved
  on public.inquiries
  for select
  to authenticated
  using (student_id = auth.uid() or supervisor_id = auth.uid());

create policy inquiries_insert_own
  on public.inquiries
  for insert
  to authenticated
  with check (
    student_id = auth.uid()
    and status = 'pending'
    and (
      inquiries.project_id is null
      or exists (
        select 1 from public.projects p
        where p.id = inquiries.project_id and p.supervisor_id = inquiries.supervisor_id
      )
    )
  );

create policy inquiries_update_involved
  on public.inquiries
  for update
  to authenticated
  using (student_id = auth.uid() or supervisor_id = auth.uid())
  with check (student_id = auth.uid() or supervisor_id = auth.uid());

create policy inquiries_admin_full_access
  on public.inquiries
  for all
  to authenticated
  using (public.current_user_role() = 'admin')
  with check (public.current_user_role() = 'admin');

-- A supervisor can read the profile of a student who sent them an inquiry.
create policy student_profiles_select_inquiring
  on public.student_profiles
  for select
  to authenticated
  using (
    exists (
      select 1 from public.inquiries i
      where i.student_id = student_profiles.user_id and i.supervisor_id = auth.uid()
    )
  );
