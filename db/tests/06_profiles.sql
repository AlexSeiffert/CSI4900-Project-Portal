-- Profiles: created automatically, and who can read and edit them.
begin;
select plan(31);

insert into auth.users (id, email, raw_user_meta_data) values
  ('00000000-0000-0000-0000-0000000000a1', 's1@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000a2', 's2@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000a3', 's3@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000b1', 'p1@t.dev', '{"role":"supervisor"}'),
  ('00000000-0000-0000-0000-0000000000b2', 'p2@t.dev', '{"role":"supervisor"}'),
  ('00000000-0000-0000-0000-0000000000c1', 'adm@t.dev', '{"role":"admin"}');

-- created automatically at signup
select is((select count(*)::int from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a1')::int, 1, 'a student signup creates a student profile');
select is((select count(*)::int from public.supervisor_profiles where user_id = '00000000-0000-0000-0000-0000000000b1')::int, 1, 'a supervisor signup creates a supervisor profile');
select is((select (select count(*) from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000c1') + (select count(*) from public.supervisor_profiles where user_id = '00000000-0000-0000-0000-0000000000c1'))::int, 0, 'an admin gets no profile');
select is((select count(*)::int from public.supervisor_profiles where user_id = '00000000-0000-0000-0000-0000000000a1')::int, 0, 'a student does not get a supervisor profile');
select is((select display_name from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a1'), 's1', 'the display name starts as the part of the email before the @');
select is((select contact_email from public.supervisor_profiles where user_id = '00000000-0000-0000-0000-0000000000b1'), 'p1@t.dev', 'a supervisor''s contact email starts as their login email');
select is((select accepting_inquiries from public.supervisor_profiles where user_id = '00000000-0000-0000-0000-0000000000b1'), true, 'a supervisor starts out accepting inquiries');
select is((select program is null from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a1'), true, 'a student program starts empty');

-- constraints
select throws_ok($$ insert into public.student_profiles (user_id, display_name) values ('00000000-0000-0000-0000-0000000000a1', 'again') $$, '23505', null, 'a second profile for the same user is rejected');
select throws_ok($$ insert into public.student_profiles (user_id, display_name) values (gen_random_uuid(), 'x') $$, '23503', null, 'a profile for a user that does not exist is rejected');
select throws_ok($$ update public.student_profiles set display_name = null where user_id = '00000000-0000-0000-0000-0000000000a1' $$, '23502', null, 'a profile needs a display name');

-- changing a role adds the new profile and keeps the old one
update public.users set role = 'supervisor' where id = '00000000-0000-0000-0000-0000000000a3';
select is((select count(*)::int from public.supervisor_profiles where user_id = '00000000-0000-0000-0000-0000000000a3')::int, 1, 'a student who becomes a supervisor gets a supervisor profile');
select is((select count(*)::int from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a3')::int, 1, 'and keeps their student profile');

-- a student
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
update public.student_profiles set program = 'CS' where user_id = '00000000-0000-0000-0000-0000000000a1';
update public.student_profiles set display_name = 'hacked' where user_id = '00000000-0000-0000-0000-0000000000a2';
delete from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a1';
select throws_ok($$ insert into public.student_profiles (user_id, display_name) values ('00000000-0000-0000-0000-0000000000a1', 'x') $$, '42501', null, 'a student cannot insert profiles directly');
select throws_ok($$ update public.student_profiles set user_id = '00000000-0000-0000-0000-0000000000a2' where user_id = '00000000-0000-0000-0000-0000000000a1' $$, '42501', null, 'a student cannot move their profile to another user');
select is((select count(*)::int from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a1')::int, 1, 'a student can read their own profile');
select is((select count(*)::int from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a2')::int, 0, 'a student cannot read another student''s profile');
select is((select count(*)::int from public.supervisor_profiles where user_id in ('00000000-0000-0000-0000-0000000000b1', '00000000-0000-0000-0000-0000000000b2'))::int, 2, 'a student can read every supervisor profile');
reset role;
select is((select program from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a1'), 'CS', 'a student can edit their own profile');
select is((select display_name from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a2'), 's2', 'a student cannot edit another student''s profile');
select is((select count(*)::int from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a1')::int, 1, 'a student cannot delete their profile');
update public.student_profiles set updated_at = now() - interval '1 day' where user_id = '00000000-0000-0000-0000-0000000000a2';
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a2","role":"authenticated"}';
update public.student_profiles set program = 'x' where user_id = '00000000-0000-0000-0000-0000000000a2';
reset role;
select is((select updated_at > now() - interval '1 hour' from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a2'), true, 'editing a profile refreshes updated_at');

-- a supervisor
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
update public.supervisor_profiles set about_me = 'hello', accepting_inquiries = false where user_id = '00000000-0000-0000-0000-0000000000b1';
update public.supervisor_profiles set about_me = 'hacked' where user_id = '00000000-0000-0000-0000-0000000000b2';
select is((select count(*)::int from public.student_profiles)::int, 0, 'a supervisor cannot read student profiles when no student has written to them');
reset role;
select is((select about_me from public.supervisor_profiles where user_id = '00000000-0000-0000-0000-0000000000b1'), 'hello', 'a supervisor can edit their own profile');
select is((select accepting_inquiries from public.supervisor_profiles where user_id = '00000000-0000-0000-0000-0000000000b1'), false, 'including turning off inquiries');
select is((select about_me from public.supervisor_profiles where user_id = '00000000-0000-0000-0000-0000000000b2'), null, 'a supervisor cannot edit another supervisor''s profile');

-- an admin and an anonymous visitor
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000c1","role":"authenticated"}';
select is((select count(*) >= 3 from public.student_profiles), true, 'an admin can read every student profile');
update public.student_profiles set program = 'by admin' where user_id = '00000000-0000-0000-0000-0000000000a1';
update public.supervisor_profiles set about_me = 'by admin' where user_id = '00000000-0000-0000-0000-0000000000b2';
reset role;
select is((select program from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a1'), 'by admin', 'an admin can edit a student''s profile');
select is((select about_me from public.supervisor_profiles where user_id = '00000000-0000-0000-0000-0000000000b2'), 'by admin', 'an admin can edit a supervisor''s profile');
set local role anon;
select is((select (select count(*) from public.student_profiles) + (select count(*) from public.supervisor_profiles))::int, 0, 'an anonymous visitor sees no profiles');
reset role;

-- deleting a user
delete from public.users where id = '00000000-0000-0000-0000-0000000000a1';
select is((select count(*)::int from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a1')::int, 0, 'deleting a user deletes their profile');

select * from finish();
rollback;
