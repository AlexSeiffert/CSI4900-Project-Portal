-- Saved projects: a student saves, lists and removes their own wishlist.
begin;
select plan(15);

insert into auth.users (id, email, raw_user_meta_data) values
  ('00000000-0000-0000-0000-0000000000a1', 's1@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000a2', 's2@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000b1', 'p1@t.dev', '{"role":"supervisor"}'),
  ('00000000-0000-0000-0000-0000000000c1', 'adm@t.dev', '{"role":"admin"}');
insert into public.projects (id, supervisor_id, title, description) values ('00000000-0000-0000-0000-0000000000d1', '00000000-0000-0000-0000-0000000000b1', 'T', 'D'), ('00000000-0000-0000-0000-0000000000d2', '00000000-0000-0000-0000-0000000000b1', 'T2', 'D');

-- saving
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
insert into public.student_saved_projects (student_id, project_id) values ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000d1');
select throws_ok($$ insert into public.student_saved_projects (student_id, project_id) values ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000d1') $$, '23505', null, 'saving the same project twice is rejected');
select throws_ok($$ insert into public.student_saved_projects (student_id, project_id) values ('00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000d1') $$, '42501', null, 'saving for another student is rejected');
select throws_ok($$ insert into public.student_saved_projects (student_id, project_id) values ('00000000-0000-0000-0000-0000000000a1', gen_random_uuid()) $$, '23503', null, 'saving a project that does not exist is rejected');
reset role;
select is((select count(*)::int from public.student_saved_projects where student_id = '00000000-0000-0000-0000-0000000000a1')::int, 1, 'a student can save a project');
select is((select saved_at is not null from public.student_saved_projects where student_id = '00000000-0000-0000-0000-0000000000a1'), true, 'a saved project records when it was saved');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
select throws_ok($$ insert into public.student_saved_projects (student_id, project_id) values ('00000000-0000-0000-0000-0000000000b1', '00000000-0000-0000-0000-0000000000d1') $$, '23503', null, 'a supervisor cannot save projects');
reset role;

-- who can see and remove
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
select is((select count(*)::int from public.student_saved_projects)::int, 1, 'a student sees their own saved projects');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a2","role":"authenticated"}';
select is((select count(*)::int from public.student_saved_projects)::int, 0, 'a student cannot see another student''s saved projects');
delete from public.student_saved_projects where student_id = '00000000-0000-0000-0000-0000000000a1';
reset role;
select is((select count(*)::int from public.student_saved_projects where student_id = '00000000-0000-0000-0000-0000000000a1')::int, 1, 'a student cannot remove another student''s saved project');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
select is((select count(*)::int from public.student_saved_projects)::int, 0, 'a supervisor cannot see students'' saved projects');
reset role;
set local role anon;
select is((select count(*)::int from public.student_saved_projects)::int, 0, 'an anonymous visitor sees no saved projects');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000c1","role":"authenticated"}';
select is((select count(*)::int from public.student_saved_projects)::int, 1, 'an admin sees every saved project');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
delete from public.student_saved_projects where student_id = '00000000-0000-0000-0000-0000000000a1' and project_id = '00000000-0000-0000-0000-0000000000d1';
reset role;
select is((select count(*)::int from public.student_saved_projects)::int, 0, 'a student can unsave a project');

-- deletions
insert into public.student_saved_projects (student_id, project_id) values ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000d1'), ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000d2');
delete from public.projects where id = '00000000-0000-0000-0000-0000000000d1';
select is((select count(*)::int from public.student_saved_projects where student_id = '00000000-0000-0000-0000-0000000000a1')::int, 1, 'deleting a project removes it from saved lists');
delete from public.users where id = '00000000-0000-0000-0000-0000000000a1';
select is((select count(*)::int from public.student_saved_projects)::int, 0, 'deleting a student deletes their saved projects');

select * from finish();
rollback;
