-- Changing and removing users: who may update or delete what.
begin;
select plan(16);

insert into auth.users (id, email, raw_user_meta_data) values
  ('00000000-0000-0000-0000-00000000000a', 'a@test.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-00000000000b', 'b@test.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-00000000000c', 'c@test.dev', '{"role":"supervisor"}'),
  ('00000000-0000-0000-0000-00000000000d', 'd@test.dev', '{"role":"admin"}'),
  ('00000000-0000-0000-0000-00000000000e', 'e@test.dev', '{"role":"student"}');

-- student A acts on their own row, on another student's row, and tries deleting
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-00000000000a","role":"authenticated"}';
update public.users set email = 'a2@test.dev' where id = '00000000-0000-0000-0000-00000000000a';
update public.users set email = 'hacked@test.dev' where id = '00000000-0000-0000-0000-00000000000b';
delete from public.users where id in ('00000000-0000-0000-0000-00000000000a', '00000000-0000-0000-0000-00000000000b');
reset role;

select is((select email from public.users where id = '00000000-0000-0000-0000-00000000000a'), 'a2@test.dev', 'a student can update their own email');
select is((select email from public.users where id = '00000000-0000-0000-0000-00000000000b'), 'b@test.dev', 'a student cannot update another user''s row');
select is((select count(*)::int from public.users where id in ('00000000-0000-0000-0000-00000000000a', '00000000-0000-0000-0000-00000000000b')), 2, 'a student cannot delete rows');

-- student E tries to promote themselves
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-00000000000e","role":"authenticated"}';
select throws_ok($$update public.users set role = 'admin' where id = '00000000-0000-0000-0000-00000000000e'$$, '42501', null, 'a student cannot change their own role');
reset role;

select is((select role::text from public.users where id = '00000000-0000-0000-0000-00000000000e'), 'student', 'and their role is unchanged');

-- admin D
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-00000000000d","role":"authenticated"}';
update public.users set email = 'b2@test.dev' where id = '00000000-0000-0000-0000-00000000000b';
update public.users set role = 'supervisor' where id = '00000000-0000-0000-0000-00000000000b';
reset role;

select is((select email from public.users where id = '00000000-0000-0000-0000-00000000000b'), 'b2@test.dev', 'an admin can update another user''s email');
select is((select role::text from public.users where id = '00000000-0000-0000-0000-00000000000b'), 'supervisor', 'an admin can change another user''s role');

set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-00000000000d","role":"authenticated"}';
select throws_ok($$update public.users set role = 'teacher' where id = '00000000-0000-0000-0000-00000000000b'$$, '22P02', null, 'an invalid role value is rejected on update');
select throws_ok($$update public.users set id = gen_random_uuid() where id = '00000000-0000-0000-0000-00000000000b'$$, '23503', null, 'an id cannot be changed to one that is not in auth.users');
delete from public.users where id = '00000000-0000-0000-0000-00000000000b';
reset role;

select is((select count(*)::int from public.users where id = '00000000-0000-0000-0000-00000000000b'), 0, 'an admin can delete another user''s row');
select is((select count(*)::int from auth.users where id = '00000000-0000-0000-0000-00000000000b'), 1, 'deleting only the public.users row leaves the login in place');

-- supervisor C
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-00000000000c","role":"authenticated"}';
update public.users set email = 'c2@test.dev' where id = '00000000-0000-0000-0000-00000000000c';
update public.users set email = 'hacked2@test.dev' where id = '00000000-0000-0000-0000-00000000000d';
select throws_ok($$update public.users set role = 'admin' where id = '00000000-0000-0000-0000-00000000000c'$$, '42501', null, 'a supervisor cannot change their own role');
reset role;

select is((select email from public.users where id = '00000000-0000-0000-0000-00000000000c'), 'c2@test.dev', 'a supervisor can update their own email');
select is((select email from public.users where id = '00000000-0000-0000-0000-00000000000d'), 'd@test.dev', 'a supervisor cannot update an admin''s row');

-- a session with no user id changes nothing
set local role authenticated;
set local request.jwt.claims = '{"role":"authenticated"}';
update public.users set email = 'hacked3@test.dev' where id = '00000000-0000-0000-0000-00000000000d';
reset role;

select is((select email from public.users where id = '00000000-0000-0000-0000-00000000000d'), 'd@test.dev', 'a session with no user id cannot update anything');

-- deleting the login removes the public.users row too
delete from auth.users where id = '00000000-0000-0000-0000-00000000000a';
select is((select count(*)::int from public.users where id = '00000000-0000-0000-0000-00000000000a'), 0, 'deleting the login also deletes the public.users row');

select * from finish();
rollback;
