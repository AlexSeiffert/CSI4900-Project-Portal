-- 0003: row level security policy on public.users
--
-- | policy                  | applies to | rule                                 |
-- | ----------------------- | ---------- | ------------------------------------ |
-- | users_select_own        | SELECT     | auth.uid() = id                      |
-- | users_update_own        | UPDATE     | auth.uid() = id                      |
-- | users_admin_full_access | ALL        | current_user_role() = 'admin'        |
--
-- current_user_role() runs as its owner (SECURITY DEFINER) so reading public.users
-- inside it skips RLS. Without that, the admin policy would call this function,
-- which would trigger the policy again, endlessly.
--
-- users_update_own lets someone update their own row, and role is part of the row, so
-- a trigger at the bottom stops anyone but an admin from changing a role.


create or replace function public.current_user_role()
returns public.user_role
language sql
security definer
stable
set search_path = public
as $$
  select role from public.users where id = auth.uid();
$$;

alter table public.users enable row level security;

create policy users_select_own
  on public.users
  for select
  to authenticated
  using (auth.uid() = id);

create policy users_update_own
  on public.users
  for update
  to authenticated
  using (auth.uid() = id)
  with check (auth.uid() = id);

create policy users_admin_full_access
  on public.users
  for all
  to authenticated
  using (public.current_user_role() = 'admin')
  with check (public.current_user_role() = 'admin');

-- Only an admin may change a role. 
create or replace function public.prevent_role_change()
returns trigger
language plpgsql
set search_path = public
as $$
begin
  if new.role is distinct from old.role
     and auth.uid() is not null
     and public.current_user_role() is distinct from 'admin' then
    raise exception 'only an admin can change a user''s role' using errcode = '42501';
  end if;

  return new;
end;
$$;

create trigger users_prevent_role_change
  before update of role on public.users
  for each row execute function public.prevent_role_change();
