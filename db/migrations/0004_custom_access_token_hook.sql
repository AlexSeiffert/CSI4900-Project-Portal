-- Runs every time someone logs in. Looks up their role in public.users and adds it
-- to their login token as `user_role`, so the app knows their role without asking
-- the database each time. Hooked up in Terraform (infra/modules/supabase-cloud/auth.tf).
-- If they have no row in public.users we leave the claim off (they can still log in, just
-- without a role); setting it to an empty value would break login entirely.

create or replace function public.custom_access_token_hook(event jsonb)
returns jsonb
language plpgsql
stable
as $$
declare
  claims jsonb;
  v_role public.user_role;
begin
  select role into v_role
  from public.users
  where id = (event ->> 'user_id')::uuid;

  claims := coalesce(event -> 'claims', '{}'::jsonb);

  if v_role is not null then
    claims := jsonb_set(claims, '{user_role}', to_jsonb(v_role));
  end if;

  event := jsonb_set(event, '{claims}', claims);

  return event;
end;
$$;

grant usage on schema public to supabase_auth_admin;
grant select on public.users to supabase_auth_admin;

grant execute on function public.custom_access_token_hook to supabase_auth_admin;
revoke execute on function public.custom_access_token_hook from authenticated, anon, public;

-- Supabase's login service runs the hook as supabase_auth_admin. The grant lets it use
-- the table, but RLS still hides every row from it, so this policy lets it read them.
create policy users_auth_admin_read
  on public.users
  for select
  to supabase_auth_admin
  using (true);
