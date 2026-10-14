-- Inquiries: who can send, see and change them, and what survives a deletion.
begin;
select plan(33);

insert into auth.users (id, email, raw_user_meta_data) values
  ('00000000-0000-0000-0000-0000000000a1', 's1@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000a2', 's2@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000b1', 'p1@t.dev', '{"role":"supervisor"}'),
  ('00000000-0000-0000-0000-0000000000b2', 'p2@t.dev', '{"role":"supervisor"}'),
  ('00000000-0000-0000-0000-0000000000c1', 'adm@t.dev', '{"role":"admin"}');
insert into public.projects (id, supervisor_id, title, description) values ('00000000-0000-0000-0000-0000000000d1', '00000000-0000-0000-0000-0000000000b1', 'T', 'D'), ('00000000-0000-0000-0000-0000000000d2', '00000000-0000-0000-0000-0000000000b2', 'Other', 'D');

-- a student sends inquiries
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
insert into public.inquiries (id, student_id, supervisor_id) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000b1');
insert into public.inquiries (id, student_id, supervisor_id, project_id) values ('00000000-0000-0000-0000-0000000000e2', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000b1', '00000000-0000-0000-0000-0000000000d1');
select throws_ok($$ insert into public.inquiries (student_id, supervisor_id, project_id) values ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000b1', '00000000-0000-0000-0000-0000000000d2') $$, '42501', null, 'an inquiry about another supervisor''s project is rejected');
select throws_ok($$ insert into public.inquiries (student_id, supervisor_id, status) values ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000b1', 'responded') $$, '42501', null, 'an inquiry cannot start as anything but pending');
select throws_ok($$ insert into public.inquiries (student_id, supervisor_id) values ('00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000b1') $$, '42501', null, 'an inquiry on behalf of another student is rejected');
select throws_ok($$ insert into public.inquiries (student_id, supervisor_id) values ('00000000-0000-0000-0000-0000000000a1', gen_random_uuid()) $$, '23503', null, 'an inquiry to a supervisor that does not exist is rejected');
reset role;
select is((select count(*)::int from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1' and project_id is null)::int, 1, 'a student can send a general inquiry');
select is((select count(*)::int from public.inquiries where id = '00000000-0000-0000-0000-0000000000e2' and project_id = '00000000-0000-0000-0000-0000000000d1')::int, 1, 'a student can send an inquiry about a project');
select is((select status::text from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1'), 'pending', 'a new inquiry starts as pending');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
select throws_ok($$ insert into public.inquiries (student_id, supervisor_id) values ('00000000-0000-0000-0000-0000000000b1', '00000000-0000-0000-0000-0000000000b2') $$, '23503', null, 'a supervisor cannot send an inquiry');
reset role;

-- who can see them
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
select is((select count(*)::int from public.inquiries)::int, 2, 'a student sees their own inquiries');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a2","role":"authenticated"}';
select is((select count(*)::int from public.inquiries)::int, 0, 'a student cannot see another student''s inquiries');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
select is((select count(*)::int from public.inquiries)::int, 2, 'a supervisor sees the inquiries sent to them');
select is((select count(*)::int from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a1')::int, 1, 'and the profile of a student who wrote to them');
select is((select count(*)::int from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a2')::int, 0, 'but not of a student who did not');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b2","role":"authenticated"}';
select is((select count(*)::int from public.inquiries)::int, 0, 'an uninvolved supervisor sees no inquiries');
select is((select count(*)::int from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a1')::int, 0, 'and cannot read that student''s profile');
reset role;
set local role anon;
select is((select count(*)::int from public.inquiries)::int, 0, 'an anonymous visitor sees no inquiries');
reset role;

-- changing an inquiry
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
update public.inquiries set status = 'viewed' where id = '00000000-0000-0000-0000-0000000000e1';
reset role;
select is((select status::text from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1'), 'viewed', 'the supervisor can change the status');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
update public.inquiries set status = 'closed' where id = '00000000-0000-0000-0000-0000000000e1';
reset role;
select is((select status::text from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1'), 'closed', 'the student can change the status too');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
select throws_ok($$ update public.inquiries set message = 'x' where id = '00000000-0000-0000-0000-0000000000e1' $$, '42501', null, 'a supervisor cannot change the message');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
select throws_ok($$ update public.inquiries set supervisor_id = '00000000-0000-0000-0000-0000000000b2' where id = '00000000-0000-0000-0000-0000000000e1' $$, '42501', null, 'an inquiry cannot be sent on to another supervisor');
select throws_ok($$ update public.inquiries set student_id = '00000000-0000-0000-0000-0000000000a2' where id = '00000000-0000-0000-0000-0000000000e1' $$, '42501', null, 'an inquiry cannot be given to another student');
select throws_ok($$ update public.inquiries set project_id = '00000000-0000-0000-0000-0000000000d1' where id = '00000000-0000-0000-0000-0000000000e1' $$, '42501', null, 'an inquiry cannot be linked to a project afterwards');
reset role;
update public.inquiries set updated_at = now() - interval '1 day' where id = '00000000-0000-0000-0000-0000000000e1';
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
update public.inquiries set status = 'responded' where id = '00000000-0000-0000-0000-0000000000e1';
reset role;
select is((select updated_at > now() - interval '1 hour' from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1'), true, 'changing the status refreshes updated_at');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a2","role":"authenticated"}';
update public.inquiries set status = 'pending' where id = '00000000-0000-0000-0000-0000000000e1';
reset role;
select is((select status::text from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1'), 'responded', 'an uninvolved student cannot change an inquiry');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b2","role":"authenticated"}';
update public.inquiries set status = 'pending' where id = '00000000-0000-0000-0000-0000000000e1';
reset role;
select is((select status::text from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1'), 'responded', 'an uninvolved supervisor cannot change an inquiry');

-- deleting
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
delete from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1';
reset role;
select is((select count(*)::int from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1')::int, 1, 'a student cannot delete an inquiry');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
delete from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1';
reset role;
select is((select count(*)::int from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1')::int, 1, 'a supervisor cannot delete an inquiry');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000c1","role":"authenticated"}';
select is((select count(*)::int from public.inquiries)::int, 2, 'an admin sees every inquiry');
update public.inquiries set message = 'edited' where id = '00000000-0000-0000-0000-0000000000e1';
reset role;
select is((select message from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1'), 'edited', 'an admin can edit an inquiry');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
delete from public.projects where id = '00000000-0000-0000-0000-0000000000d1';
reset role;
select is((select count(*)::int from public.inquiries where id = '00000000-0000-0000-0000-0000000000e2' and project_id is null)::int, 1, 'deleting a project keeps its inquiries, with no project');
select is((select count(*)::int from public.inquiries)::int, 2, 'and no inquiry is lost');
delete from public.users where id = '00000000-0000-0000-0000-0000000000a1';
select is((select count(*)::int from public.inquiries where student_id = '00000000-0000-0000-0000-0000000000a1')::int, 0, 'deleting a student deletes their inquiries');
insert into public.inquiries (id, student_id, supervisor_id) values ('00000000-0000-0000-0000-0000000000e3', '00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000b2');
delete from public.users where id = '00000000-0000-0000-0000-0000000000b2';
select is((select count(*)::int from public.inquiries where id = '00000000-0000-0000-0000-0000000000e3')::int, 0, 'deleting a supervisor deletes the inquiries sent to them');

select * from finish();
rollback;
