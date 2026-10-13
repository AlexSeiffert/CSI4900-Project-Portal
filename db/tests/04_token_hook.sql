-- The login hook: what it puts on the token, and who may call it.
begin;
select plan(11);

insert into auth.users (id, email, raw_user_meta_data) values
  ('00000000-0000-0000-0000-00000000000a', 'a@test.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-00000000000c', 'c@test.dev', '{"role":"supervisor"}'),
  ('00000000-0000-0000-0000-00000000000d', 'd@test.dev', '{"role":"admin"}');

create function pg_temp.hook(uid uuid) returns jsonb language sql as $$
  select public.custom_access_token_hook(
    jsonb_build_object('user_id', uid, 'claims', jsonb_build_object('role', 'authenticated'))
  ) -> 'claims'
$$;

select is(pg_temp.hook('00000000-0000-0000-0000-00000000000a') ->> 'user_role', 'student', 'a student''s token carries user_role student');
select is(pg_temp.hook('00000000-0000-0000-0000-00000000000c') ->> 'user_role', 'supervisor', 'a supervisor''s token carries user_role supervisor');
select is(pg_temp.hook('00000000-0000-0000-0000-00000000000d') ->> 'user_role', 'admin', 'an admin''s token carries user_role admin');
select is(pg_temp.hook('00000000-0000-0000-0000-00000000000a') ->> 'role', 'authenticated', 'the claims it was given are kept');

select is(pg_temp.hook(gen_random_uuid()) ->> 'user_role', null, 'an unknown user gets no user_role claim');
select is(pg_temp.hook(gen_random_uuid()) ->> 'role', 'authenticated', 'and their other claims are intact');
select is(jsonb_typeof(public.custom_access_token_hook(jsonb_build_object('user_id', '00000000-0000-0000-0000-00000000000a')) -> 'claims'), 'object', 'an event with no claims still gets a claims object');

update public.users set role = 'admin' where id = '00000000-0000-0000-0000-00000000000a';
select is(pg_temp.hook('00000000-0000-0000-0000-00000000000a') ->> 'user_role', 'admin', 'a role change shows up the next time a token is issued');

delete from public.users where id = '00000000-0000-0000-0000-00000000000c';
select is(pg_temp.hook('00000000-0000-0000-0000-00000000000c') ->> 'user_role', null, 'a login whose public.users row is gone gets no claim, and no error');

set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-00000000000a","role":"authenticated"}';
select throws_ok($$select public.custom_access_token_hook('{}'::jsonb)$$, '42501', null, 'a signed-in user cannot call the hook');
reset role;

set local role anon;
select throws_ok($$select public.custom_access_token_hook('{}'::jsonb)$$, '42501', null, 'an anonymous visitor cannot call the hook');
reset role;

select * from finish();
rollback;
