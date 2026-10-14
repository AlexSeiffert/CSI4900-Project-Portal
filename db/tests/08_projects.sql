-- Projects: who can create, edit, change the status of and delete them.
begin;
select plan(21);

insert into auth.users (id, email, raw_user_meta_data) values
  ('00000000-0000-0000-0000-0000000000b1', 'p1@t.dev', '{"role":"supervisor"}'),
  ('00000000-0000-0000-0000-0000000000b2', 'p2@t.dev', '{"role":"supervisor"}'),
  ('00000000-0000-0000-0000-0000000000a1', 's1@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000c1', 'adm@t.dev', '{"role":"admin"}');
insert into public.projects (id, supervisor_id, title, description) values ('00000000-0000-0000-0000-0000000000d2', '00000000-0000-0000-0000-0000000000b2', 'Other', 'Other project');

-- a supervisor creates a project
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
insert into public.projects (id, supervisor_id, title, description) values ('00000000-0000-0000-0000-0000000000d1', '00000000-0000-0000-0000-0000000000b1', 'T', 'D');
select throws_ok($$ insert into public.projects (supervisor_id, title, description) values ('00000000-0000-0000-0000-0000000000b2', 'T', 'D') $$, '42501', null, 'a supervisor cannot create a project for someone else');
select throws_ok($$ insert into public.projects (supervisor_id, description) values ('00000000-0000-0000-0000-0000000000b1', 'D') $$, '23502', null, 'a project needs a title');
select throws_ok($$ insert into public.projects (supervisor_id, title) values ('00000000-0000-0000-0000-0000000000b1', 'T') $$, '23502', null, 'a project needs a description');
select throws_ok($$ insert into public.projects (supervisor_id, title, description, status) values ('00000000-0000-0000-0000-0000000000b1', 'T', 'D', 'finished') $$, '22P02', null, 'an unknown status is rejected');
reset role;
select is((select count(*)::int from public.projects where id = '00000000-0000-0000-0000-0000000000d1')::int, 1, 'a supervisor can create their own project');
select is((select status::text from public.projects where id = '00000000-0000-0000-0000-0000000000d1'), 'available', 'a new project starts as available');
select is((select posted_at is not null from public.projects where id = '00000000-0000-0000-0000-0000000000d1'), true, 'a new project gets a posted date');

-- another supervisor and a student
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b2","role":"authenticated"}';
update public.projects set title = 'hacked' where id = '00000000-0000-0000-0000-0000000000d1';
delete from public.projects where id = '00000000-0000-0000-0000-0000000000d1';
reset role;
select is((select title from public.projects where id = '00000000-0000-0000-0000-0000000000d1'), 'T', 'another supervisor cannot edit a project');
select is((select count(*)::int from public.projects where id = '00000000-0000-0000-0000-0000000000d1')::int, 1, 'another supervisor cannot delete a project');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
select is((select count(*)::int from public.projects)::int, 2, 'a student can read every project');
select throws_ok($$ insert into public.projects (supervisor_id, title, description) values ('00000000-0000-0000-0000-0000000000b1', 'T', 'D') $$, '42501', null, 'a student cannot create a project');
update public.projects set title = 'student edit' where id = '00000000-0000-0000-0000-0000000000d1';
delete from public.projects where id = '00000000-0000-0000-0000-0000000000d1';
reset role;
select is((select title from public.projects where id = '00000000-0000-0000-0000-0000000000d1'), 'T', 'a student cannot edit a project');
select is((select count(*)::int from public.projects where id = '00000000-0000-0000-0000-0000000000d1')::int, 1, 'a student cannot delete a project');

-- the owner edits
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
update public.projects set status = 'archived' where id = '00000000-0000-0000-0000-0000000000d1';
reset role;
select is((select status::text||':'||title from public.projects where id = '00000000-0000-0000-0000-0000000000d1'), 'archived:T', 'a supervisor can change just the status');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
update public.projects set title = 'Edited', description = 'New text', external_url = 'https://example.org' where id = '00000000-0000-0000-0000-0000000000d1';
select throws_ok($$ update public.projects set supervisor_id = '00000000-0000-0000-0000-0000000000b2' where id = '00000000-0000-0000-0000-0000000000d1' $$, '42501', null, 'a supervisor cannot hand a project to someone else');
reset role;
select is((select title||':'||description||':'||external_url from public.projects where id = '00000000-0000-0000-0000-0000000000d1'), 'Edited:New text:https://example.org', 'a supervisor can edit their own project');
update public.projects set updated_at = now() - interval '1 day' where id = '00000000-0000-0000-0000-0000000000d1';
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
update public.projects set title = 'Edited again' where id = '00000000-0000-0000-0000-0000000000d1';
reset role;
select is((select updated_at > now() - interval '1 hour' from public.projects where id = '00000000-0000-0000-0000-0000000000d1'), true, 'editing a project refreshes updated_at');

-- an anonymous visitor and an admin
set local role anon;
select is((select count(*)::int from public.projects)::int, 0, 'an anonymous visitor sees no projects');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000c1","role":"authenticated"}';
update public.projects set title = 'by admin' where id = '00000000-0000-0000-0000-0000000000d1';
delete from public.projects where id = '00000000-0000-0000-0000-0000000000d2';
reset role;
select is((select title from public.projects where id = '00000000-0000-0000-0000-0000000000d1'), 'by admin', 'an admin can edit any project');
select is((select count(*)::int from public.projects where id = '00000000-0000-0000-0000-0000000000d2')::int, 0, 'an admin can delete any project');

-- deleting a supervisor
delete from public.users where id = '00000000-0000-0000-0000-0000000000b1';
select is((select count(*)::int from public.projects where id = '00000000-0000-0000-0000-0000000000d1')::int, 0, 'deleting a supervisor deletes their projects');

select * from finish();
rollback;
