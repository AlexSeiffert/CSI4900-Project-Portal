-- Adding users: what signup creates, and what inserts must be rejected.
begin;
select plan(17);

insert into auth.users (id, email, raw_user_meta_data) values
  ('00000000-0000-0000-0000-00000000000a', 'a@test.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-00000000000c', 'c@test.dev', '{"role":"supervisor"}'),
  ('00000000-0000-0000-0000-00000000000d', 'd@test.dev', '{"role":"admin"}');

-- signup creates the matching public.users row
select is((select role::text from public.users where id = '00000000-0000-0000-0000-00000000000a'), 'student', 'student signup gets role student');
select is((select role::text from public.users where id = '00000000-0000-0000-0000-00000000000c'), 'supervisor', 'supervisor signup gets role supervisor');
select is((select role::text from public.users where id = '00000000-0000-0000-0000-00000000000d'), 'admin', 'admin signup gets role admin');
select is((select email from public.users where id = '00000000-0000-0000-0000-00000000000a'), 'a@test.dev', 'signup copies the email');
select is((select count(*)::int from public.users where id = '00000000-0000-0000-0000-00000000000a'), 1, 'signup creates exactly one row');

-- bad signups fail and leave nothing behind
select throws_ok($$insert into auth.users (id, email, raw_user_meta_data) values (gen_random_uuid(), 'bad@test.dev', '{"role":"teacher"}')$$, '22P02', null, 'an invalid role is rejected');
select is((select count(*)::int from auth.users where email = 'bad@test.dev'), 0, 'an invalid role leaves no login behind');
select throws_ok($$insert into auth.users (id, email, raw_user_meta_data) values (gen_random_uuid(), 'none@test.dev', '{}')$$, '23502', null, 'a missing role is rejected');
select is((select count(*)::int from auth.users where email = 'none@test.dev'), 0, 'a missing role leaves no login behind');
select throws_ok($$insert into auth.users (id, email, raw_user_meta_data) values (gen_random_uuid(), 'a@test.dev', '{"role":"student"}')$$, '23505', null, 'a duplicate email is rejected');

-- inserting into public.users directly
select throws_ok($$insert into public.users (id, email, role) values ('00000000-0000-0000-0000-00000000000a', 'dup@test.dev', 'student')$$, '23505', null, 'a duplicate id is rejected');
select throws_ok($$insert into public.users (id, email, role) values (gen_random_uuid(), 'x@test.dev', 'student')$$, '23503', null, 'an id that is not in auth.users is rejected');
select throws_ok($$insert into public.users (id, email, role) values (gen_random_uuid(), null, 'student')$$, '23502', null, 'a null email is rejected');
select throws_ok($$insert into public.users (id, email, role) values (gen_random_uuid(), 'x@test.dev', null)$$, '23502', null, 'a null role is rejected');
select throws_ok($$insert into public.users (id, email, role) values (gen_random_uuid(), 'x@test.dev', 'teacher')$$, '22P02', null, 'an invalid role value is rejected');

-- signed-in students and anonymous visitors cannot insert
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-00000000000a","role":"authenticated"}';
select throws_ok($$insert into public.users (id, email, role) values (gen_random_uuid(), 'x@test.dev', 'admin')$$, '42501', null, 'a student cannot insert rows');
reset role;

set local role anon;
select throws_ok($$insert into public.users (id, email, role) values (gen_random_uuid(), 'x@test.dev', 'admin')$$, '42501', null, 'an anonymous visitor cannot insert rows');
reset role;

select * from finish();
rollback;
