-- 0002: provisions the public.users row when someone signs up
--
-- Copies the role chosen at signup (raw_user_meta_data.role) into public.users.
-- If the role is missing or isn't student, supervisor or admin, the signup fails
-- and nothing is created.

create or replace function public.handle_new_user()
returns trigger
language plpgsql
security definer
set search_path = public
as $$
declare
  new_role public.user_role;
begin
  new_role := (new.raw_user_meta_data ->> 'role')::public.user_role;

  insert into public.users (id, email, role)
  values (new.id, new.email, new_role);

  return new;
end;
$$;

create trigger on_auth_user_created
  after insert on auth.users
  for each row execute function public.handle_new_user();
