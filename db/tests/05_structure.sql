-- Structure: the tables, functions, policies and grants the migrations should have created.
begin;
select plan(33);

-- 0001 users table and role enum
select enum_has_labels('public', 'user_role', array['student', 'supervisor', 'admin'], 'user_role has exactly the three roles');
select has_table('public', 'users', 'public.users exists');
select columns_are('public', 'users', array['id', 'email', 'role'], 'public.users has exactly id, email, role');
select col_type_is('public', 'users', 'id', 'uuid', 'id is a uuid');
select col_type_is('public', 'users', 'email', 'text', 'email is text');
select col_type_is('public', 'users', 'role', 'public', 'user_role', 'role is the user_role enum');
select col_not_null('public', 'users', 'email', 'email is required');
select col_not_null('public', 'users', 'role', 'role is required');
select col_is_pk('public', 'users', 'id', 'id is the primary key');
select fk_ok('public', 'users', 'id', 'auth', 'users', 'id', 'id references auth.users(id)');

-- 0002 signup trigger
select has_trigger('auth', 'users', 'on_auth_user_created', 'the signup trigger exists on auth.users');
select trigger_is('auth', 'users', 'on_auth_user_created', 'public', 'handle_new_user', 'and it runs handle_new_user()');
select is_definer('public', 'handle_new_user', 'handle_new_user() is SECURITY DEFINER');
select function_returns('public', 'handle_new_user', 'trigger', 'handle_new_user() returns a trigger');

-- 0003 row level security
select ok((select relrowsecurity from pg_class where oid = 'public.users'::regclass), 'RLS is enabled on public.users');
select policies_are('public', 'users', array['users_admin_full_access', 'users_auth_admin_read', 'users_select_own', 'users_update_own'], 'public.users has exactly the expected policies');
select policy_cmd_is('public', 'users', 'users_select_own', 'SELECT', 'users_select_own covers SELECT');
select policy_cmd_is('public', 'users', 'users_update_own', 'UPDATE', 'users_update_own covers UPDATE');
select policy_cmd_is('public', 'users', 'users_admin_full_access', 'ALL', 'users_admin_full_access covers ALL');
select policy_cmd_is('public', 'users', 'users_auth_admin_read', 'SELECT', 'users_auth_admin_read covers SELECT');
select policy_roles_are('public', 'users', 'users_select_own', array['authenticated'], 'users_select_own applies to authenticated');
select policy_roles_are('public', 'users', 'users_update_own', array['authenticated'], 'users_update_own applies to authenticated');
select policy_roles_are('public', 'users', 'users_admin_full_access', array['authenticated'], 'users_admin_full_access applies to authenticated');
select policy_roles_are('public', 'users', 'users_auth_admin_read', array['supabase_auth_admin'], 'users_auth_admin_read applies to supabase_auth_admin');
select is_definer('public', 'current_user_role', 'current_user_role() is SECURITY DEFINER');
select has_trigger('public', 'users', 'users_prevent_role_change', 'the role-protection trigger exists on public.users');
select trigger_is('public', 'users', 'users_prevent_role_change', 'public', 'prevent_role_change', 'and it runs prevent_role_change()');

-- 0004 token hook
select function_returns('public', 'custom_access_token_hook', array['jsonb'], 'jsonb', 'the hook takes and returns jsonb');
select function_privs_are('public', 'custom_access_token_hook', array['jsonb'], 'supabase_auth_admin', array['EXECUTE'], 'supabase_auth_admin can call the hook');
select function_privs_are('public', 'custom_access_token_hook', array['jsonb'], 'authenticated', array[]::text[], 'signed-in users cannot call the hook');
select function_privs_are('public', 'custom_access_token_hook', array['jsonb'], 'anon', array[]::text[], 'anonymous visitors cannot call the hook');
select table_privs_are('public', 'users', 'supabase_auth_admin', array['SELECT'], 'supabase_auth_admin can only select from public.users');
select ok(has_schema_privilege('supabase_auth_admin', 'public', 'usage'), 'supabase_auth_admin can use the public schema');

select * from finish();
rollback;
