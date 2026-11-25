-- 0009: inquiries, a group of up to three students asking a supervisor about a project
--
-- One row per inquiry, plus one inquiry_members row per student in the group. The student
-- sends the email themselves (from Outlook); the app only tracks the request and its outcome.
--
-- inquiries
-- | column        | type           | notes                                                  |
-- | ------------- | -------------- | ------------------------------------------------------ |
-- | id            | uuid           | PK                                                     |
-- | student_id    | uuid           | the creator; FK -> student_profiles(user_id)           |
-- | supervisor_id | uuid           | FK -> supervisor_profiles(user_id)                     |
-- | project_id    | uuid           | optional (null = a general inquiry); FK -> projects     |
-- | status        | inquiry_status | draft / sent / offered / matched / closed              |
-- | message       | text           | optional, at most 2000 characters                      |
-- | created_at    | timestamptz    |                                                        |
-- | updated_at    | timestamptz    | kept current by set_updated_at()                       |
--
-- inquiry_members
-- | column     | type             | notes                                       |
-- | ---------- | ---------------- | ------------------------------------------- |
-- | inquiry_id | uuid             | FK -> inquiries(id)                         |
-- | student_id | uuid             | FK -> student_profiles(user_id)             |
-- | response   | inquiry_response | pending / accepted / declined               |
--
-- The primary key is (inquiry_id, student_id). An inquiry has at most 3 members, the creator
-- included. The creator adds friends by email (find_student_by_email) while it is a draft.
--
-- If the project is later deleted, the inquiry stays and project_id becomes null.
--
-- Status changes:
-- | change              | who                                                                      |
-- | ------------------- | ------------------------------------------------------------------------ |
-- | draft -> sent       | a member, once they have sent the email                                  |
-- | sent -> offered     | the supervisor                                                           |
-- | any -> closed       | the supervisor                                                           |
-- | offered -> matched  | a member, once nobody is pending and at least one member has accepted    |
-- | offered -> closed   | a member, once nobody is pending and nobody has accepted                 |
--
-- So only a student's confirmation can produce matched. A member who declines is removed from
-- the inquiry. Only an admin may change anything else.

create type public.inquiry_status as enum ('draft', 'sent', 'offered', 'matched', 'closed');
create type public.inquiry_response as enum ('pending', 'accepted', 'declined');

create table public.inquiries (
  id uuid primary key default gen_random_uuid(),
  student_id uuid not null references public.student_profiles (user_id) on delete cascade,
  supervisor_id uuid not null references public.supervisor_profiles (user_id) on delete cascade,
  project_id uuid references public.projects (id) on delete set null,
  status public.inquiry_status not null default 'draft',
  message text check (char_length(message) <= 2000),
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create index inquiries_student_id_idx on public.inquiries (student_id);
create index inquiries_supervisor_id_idx on public.inquiries (supervisor_id);
create index inquiries_project_id_idx on public.inquiries (project_id);

create trigger inquiries_set_updated_at
  before update on public.inquiries
  for each row execute function public.set_updated_at();

create table public.inquiry_members (
  inquiry_id uuid not null references public.inquiries (id) on delete cascade,
  student_id uuid not null references public.student_profiles (user_id) on delete cascade,
  response public.inquiry_response not null default 'pending',
  primary key (inquiry_id, student_id)
);

create index inquiry_members_student_id_idx on public.inquiry_members (student_id);

-- SECURITY DEFINER so policies on inquiries and inquiry_members can ask about each other
-- without each one's RLS calling the other's, endlessly.
create or replace function public.is_inquiry_member(p_inquiry_id uuid)
returns boolean
language sql
security definer
stable
set search_path = public
as $$
  select exists (
    select 1 from public.inquiry_members
    where inquiry_id = p_inquiry_id and student_id = auth.uid()
  );
$$;

create or replace function public.is_inquiry_supervisor(p_inquiry_id uuid)
returns boolean
language sql
security definer
stable
set search_path = public
as $$
  select exists (
    select 1 from public.inquiries
    where id = p_inquiry_id and supervisor_id = auth.uid()
  );
$$;

-- Enforces the status table above. Admins and the database itself (no auth.uid()) skip it.
create or replace function public.limit_inquiry_updates()
returns trigger
language plpgsql
security definer
set search_path = public
as $$
declare
  pending int;
  accepted int;
begin
  if auth.uid() is null or public.current_user_role() = 'admin' then
    return new;
  end if;

  if (new.student_id, new.supervisor_id, new.message, new.created_at)
       is distinct from (old.student_id, old.supervisor_id, old.message, old.created_at)
     or (new.project_id is not null and new.project_id is distinct from old.project_id) then
    raise exception 'only the status of an inquiry can be changed' using errcode = '42501';
  end if;

  if new.status is not distinct from old.status then
    return new;
  end if;

  select count(*) filter (where response = 'pending'), count(*) filter (where response = 'accepted')
    into pending, accepted
  from public.inquiry_members where inquiry_id = new.id;

  if old.supervisor_id = auth.uid() then
    if (old.status = 'sent' and new.status = 'offered') or new.status = 'closed' then
      return new;
    end if;
  elsif exists (select 1 from public.inquiry_members where inquiry_id = new.id and student_id = auth.uid()) then
    if (old.status = 'draft' and new.status = 'sent')
       or (old.status = 'offered' and new.status = 'matched' and pending = 0 and accepted > 0)
       or (old.status = 'offered' and new.status = 'closed' and pending = 0 and accepted = 0) then
      return new;
    end if;
  end if;

  raise exception 'an inquiry cannot go from % to % here', old.status, new.status using errcode = '42501';
end;
$$;

create trigger inquiries_limit_updates
  before update on public.inquiries
  for each row execute function public.limit_inquiry_updates();

-- At most 3 members, and a new member always starts as pending. Locks the inquiry so two
-- friends added at once can't both squeeze in.
create or replace function public.limit_inquiry_members()
returns trigger
language plpgsql
security definer
set search_path = public
as $$
begin
  perform 1 from public.inquiries where id = new.inquiry_id for update;
  if (select count(*) from public.inquiry_members where inquiry_id = new.inquiry_id) >= 3 then
    raise exception 'an inquiry can have at most 3 members' using errcode = '23514';
  end if;
  if new.response <> 'pending' and auth.uid() is not null and public.current_user_role() is distinct from 'admin' then
    raise exception 'a new member starts as pending' using errcode = '42501';
  end if;
  return new;
end;
$$;

create trigger inquiry_members_limit
  before insert on public.inquiry_members
  for each row execute function public.limit_inquiry_members();

-- A member may only answer an offer, once: pending -> accepted or declined.
create or replace function public.limit_inquiry_member_updates()
returns trigger
language plpgsql
security definer
set search_path = public
as $$
begin
  if auth.uid() is null or public.current_user_role() = 'admin' then
    return new;
  end if;
  if (new.inquiry_id, new.student_id) is distinct from (old.inquiry_id, old.student_id)
     or old.response <> 'pending'
     or (select status from public.inquiries where id = new.inquiry_id) <> 'offered' then
    raise exception 'a member can only answer an open offer, once' using errcode = '42501';
  end if;
  return new;
end;
$$;

create trigger inquiry_members_limit_updates
  before update on public.inquiry_members
  for each row execute function public.limit_inquiry_member_updates();

-- Turns a friend's email into their id without letting the caller read public.users.
create or replace function public.find_student_by_email(p_email text)
returns uuid
language plpgsql
security definer
stable
set search_path = public
as $$
declare
  found uuid;
begin
  select id into found from public.users
  where lower(email) = lower(trim(p_email)) and role = 'student';
  if found is null then
    raise exception 'not found' using errcode = 'P0002';
  end if;
  return found;
end;
$$;

revoke execute on function public.find_student_by_email(text) from public, anon;
grant execute on function public.find_student_by_email(text) to authenticated;

-- The members' emails, for the CC line. Only the members and the supervisor get any rows.
create or replace function public.inquiry_member_emails(p_inquiry_id uuid)
returns table (student_id uuid, email text)
language sql
security definer
stable
set search_path = public
as $$
  select m.student_id, u.email
  from public.inquiry_members m
  join public.users u on u.id = m.student_id
  where m.inquiry_id = p_inquiry_id
    and (public.is_inquiry_member(p_inquiry_id) or public.is_inquiry_supervisor(p_inquiry_id)
         or public.current_user_role() = 'admin');
$$;

revoke execute on function public.inquiry_member_emails(uuid) from public, anon;
grant execute on function public.inquiry_member_emails(uuid) to authenticated;

alter table public.inquiries enable row level security;
alter table public.inquiry_members enable row level security;

-- The creator can always see their own inquiry, so inserting it can return the new row
-- before the creator's member row exists.
create policy inquiries_select_involved
  on public.inquiries
  for select
  to authenticated
  using (student_id = auth.uid() or supervisor_id = auth.uid() or public.is_inquiry_member(id));

create policy inquiries_insert_own
  on public.inquiries
  for insert
  to authenticated
  with check (
    student_id = auth.uid()
    and status = 'draft'
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
  using (supervisor_id = auth.uid() or public.is_inquiry_member(id))
  with check (supervisor_id = auth.uid() or public.is_inquiry_member(id));

create policy inquiries_admin_full_access
  on public.inquiries
  for all
  to authenticated
  using (public.current_user_role() = 'admin')
  with check (public.current_user_role() = 'admin');

create policy inquiry_members_select_involved
  on public.inquiry_members
  for select
  to authenticated
  using (
    student_id = auth.uid()
    or public.is_inquiry_member(inquiry_id)
    or public.is_inquiry_supervisor(inquiry_id)
  );

-- The creator adds members (themselves included) while the inquiry is still a draft.
create policy inquiry_members_insert_by_creator
  on public.inquiry_members
  for insert
  to authenticated
  with check (
    exists (
      select 1 from public.inquiries i
      where i.id = inquiry_members.inquiry_id and i.student_id = auth.uid() and i.status = 'draft'
    )
  );

create policy inquiry_members_update_own
  on public.inquiry_members
  for update
  to authenticated
  using (student_id = auth.uid())
  with check (student_id = auth.uid());

-- A member can leave at any time; the creator can also remove someone from a draft.
create policy inquiry_members_delete_own
  on public.inquiry_members
  for delete
  to authenticated
  using (
    student_id = auth.uid()
    or exists (
      select 1 from public.inquiries i
      where i.id = inquiry_members.inquiry_id and i.student_id = auth.uid() and i.status = 'draft'
    )
  );

create policy inquiry_members_admin_full_access
  on public.inquiry_members
  for all
  to authenticated
  using (public.current_user_role() = 'admin')
  with check (public.current_user_role() = 'admin');

-- A supervisor can read the profiles of the members (and creator) of an inquiry sent to them,
-- and members can read each other's.
create policy student_profiles_select_inquiring
  on public.student_profiles
  for select
  to authenticated
  using (
    exists (
      select 1 from public.inquiry_members m
      where m.student_id = student_profiles.user_id
        and (public.is_inquiry_supervisor(m.inquiry_id) or public.is_inquiry_member(m.inquiry_id))
    )
    or exists (
      select 1 from public.inquiries i
      where i.student_id = student_profiles.user_id
        and (i.supervisor_id = auth.uid() or public.is_inquiry_member(i.id))
    )
  );
