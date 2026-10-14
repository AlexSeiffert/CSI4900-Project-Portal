-- 0005: student and supervisor profiles
--
-- One profile table per role, each strictly 1:1 with public.users.
--
-- | table               | column              | type        | notes                                   |
-- | ------------------- | ------------------- | ----------- | --------------------------------------- |
-- | student_profiles    | user_id             | uuid        | PK, FK -> public.users(id)              |
-- |                     | display_name        | text        | starts as the part of the email before @ |
-- |                     | program             | text        | optional                                |
-- | supervisor_profiles | user_id             | uuid        | PK, FK -> public.users(id)              |
-- |                     | display_name        | text        | starts as the part of the email before @ |
-- |                     | contact_email       | text        | starts as the login email               |
-- |                     | external_url        | text        | optional                                |
-- |                     | about_me            | text        | optional                                |
-- |                     | accepting_inquiries | boolean     | default true                            |
--
-- Rows are created automatically when a user gets the matching role (see the trigger
-- below), so nobody inserts them by hand. Changing a user's role later adds the new
-- profile but keeps the old one.
--
-- Supervisor profiles are readable by every signed-in user, because students browse them.
-- Student profiles are readable only by their owner (and, from 0009, by supervisors who
-- received an inquiry from that student). Owners can update their own profile.

-- Keeps updated_at current on any row change. Also used by later tables.
create or replace function public.set_updated_at()
returns trigger
language plpgsql
as $$
begin
  new.updated_at = now();
  return new;
end;
$$;

create table public.student_profiles (
  user_id uuid primary key references public.users (id) on delete cascade,
  display_name text not null,
  program text,
  updated_at timestamptz not null default now()
);

create table public.supervisor_profiles (
  user_id uuid primary key references public.users (id) on delete cascade,
  display_name text not null,
  contact_email text not null,
  external_url text,
  about_me text,
  accepting_inquiries boolean not null default true,
  updated_at timestamptz not null default now()
);

create trigger student_profiles_set_updated_at
  before update on public.student_profiles
  for each row execute function public.set_updated_at();

create trigger supervisor_profiles_set_updated_at
  before update on public.supervisor_profiles
  for each row execute function public.set_updated_at();

-- Creates the matching profile when a user is added or gets the student/supervisor role.
-- SECURITY DEFINER because the person signing up has no right to write to the profile tables.
create or replace function public.handle_new_profile()
returns trigger
language plpgsql
security definer
set search_path = public
as $$
begin
  if new.role = 'student' then
    insert into public.student_profiles (user_id, display_name)
    values (new.id, split_part(new.email, '@', 1))
    on conflict (user_id) do nothing;
  elsif new.role = 'supervisor' then
    insert into public.supervisor_profiles (user_id, display_name, contact_email)
    values (new.id, split_part(new.email, '@', 1), new.email)
    on conflict (user_id) do nothing;
  end if;

  return new;
end;
$$;

create trigger users_create_profile
  after insert or update of role on public.users
  for each row execute function public.handle_new_profile();

-- Profiles for users that already exist
insert into public.student_profiles (user_id, display_name)
select id, split_part(email, '@', 1) from public.users where role = 'student'
on conflict (user_id) do nothing;

insert into public.supervisor_profiles (user_id, display_name, contact_email)
select id, split_part(email, '@', 1), email from public.users where role = 'supervisor'
on conflict (user_id) do nothing;

alter table public.student_profiles enable row level security;
alter table public.supervisor_profiles enable row level security;

create policy student_profiles_select_own
  on public.student_profiles
  for select
  to authenticated
  using (auth.uid() = user_id);

create policy student_profiles_update_own
  on public.student_profiles
  for update
  to authenticated
  using (auth.uid() = user_id)
  with check (auth.uid() = user_id);

create policy student_profiles_admin_full_access
  on public.student_profiles
  for all
  to authenticated
  using (public.current_user_role() = 'admin')
  with check (public.current_user_role() = 'admin');

create policy supervisor_profiles_select_all
  on public.supervisor_profiles
  for select
  to authenticated
  using (true);

create policy supervisor_profiles_update_own
  on public.supervisor_profiles
  for update
  to authenticated
  using (auth.uid() = user_id)
  with check (auth.uid() = user_id);

create policy supervisor_profiles_admin_full_access
  on public.supervisor_profiles
  for all
  to authenticated
  using (public.current_user_role() = 'admin')
  with check (public.current_user_role() = 'admin');
