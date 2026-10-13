-- Reading users: what each kind of caller can see.
begin;
select plan(8);

insert into auth.users (id, email, raw_user_meta_data) values
  ('00000000-0000-0000-0000-00000000000a', 'a@test.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-00000000000b', 'b@test.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-00000000000c', 'c@test.dev', '{"role":"supervisor"}'),
  ('00000000-0000-0000-0000-00000000000d', 'd@test.dev', '{"role":"admin"}');

set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-00000000000a","role":"authenticated"}';
select is((select count(*)::int from public.users), 1, 'a student sees exactly one row');
select is((select id from public.users), '00000000-0000-0000-0000-00000000000a'::uuid, 'and it is their own');

set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-00000000000c","role":"authenticated"}';
select is((select count(*)::int from public.users), 1, 'a supervisor sees exactly one row');
select is((select id from public.users), '00000000-0000-0000-0000-00000000000c'::uuid, 'and it is their own');

set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-00000000000d","role":"authenticated"}';
select cmp_ok((select count(*)::int from public.users), '>=', 4, 'an admin sees every row');
select is((select count(*)::int from public.users where id = '00000000-0000-0000-0000-00000000000a'), 1, 'including other users');

set local request.jwt.claims = '{"role":"authenticated"}';
select is((select count(*)::int from public.users), 0, 'a session with no user id sees nothing');
reset role;

set local role anon;
select is((select count(*)::int from public.users), 0, 'an anonymous visitor sees nothing');
reset role;

select * from finish();
rollback;
