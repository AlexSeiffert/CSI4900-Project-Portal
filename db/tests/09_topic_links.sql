-- Topic links: project tags, supervisor interests and student interests.
begin;
select plan(27);

insert into auth.users (id, email, raw_user_meta_data) values
  ('00000000-0000-0000-0000-0000000000b1', 'p1@t.dev', '{"role":"supervisor"}'),
  ('00000000-0000-0000-0000-0000000000b2', 'p2@t.dev', '{"role":"supervisor"}'),
  ('00000000-0000-0000-0000-0000000000a1', 's1@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000a2', 's2@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000c1', 'adm@t.dev', '{"role":"admin"}');
insert into public.projects (id, supervisor_id, title, description) values ('00000000-0000-0000-0000-0000000000d1', '00000000-0000-0000-0000-0000000000b1', 'T', 'D');
do $$ begin perform public.get_or_create_topic('NLP'); perform public.get_or_create_topic('Robotics'); end $$;

-- project tags
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
insert into public.project_topic_tags (project_id, topic_id) values ('00000000-0000-0000-0000-0000000000d1', public.get_or_create_topic('NLP'));
select throws_ok($$ insert into public.project_topic_tags (project_id, topic_id) values ('00000000-0000-0000-0000-0000000000d1', public.get_or_create_topic('NLP')) $$, '23505', null, 'tagging a project twice with the same topic is rejected');
select throws_ok($$ insert into public.project_topic_tags (project_id, topic_id) values ('00000000-0000-0000-0000-0000000000d1', gen_random_uuid()) $$, '23503', null, 'a tag needs an existing topic');
select throws_ok($$ insert into public.project_topic_tags (project_id, topic_id) values (gen_random_uuid(), public.get_or_create_topic('NLP')) $$, '42501', null, 'a tag needs one of your own projects');
reset role;
select is((select count(*)::int from public.project_topic_tags where project_id = '00000000-0000-0000-0000-0000000000d1')::int, 1, 'a supervisor can tag their own project');
select is((select t.name from public.project_topic_tags l join public.topics t on t.id = l.topic_id where l.project_id = '00000000-0000-0000-0000-0000000000d1'), 'NLP', 'a project''s tags come back with their topic names');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b2","role":"authenticated"}';
select throws_ok($$ insert into public.project_topic_tags (project_id, topic_id) values ('00000000-0000-0000-0000-0000000000d1', public.get_or_create_topic('Robotics')) $$, '42501', null, 'a supervisor cannot tag another supervisor''s project');
delete from public.project_topic_tags where project_id = '00000000-0000-0000-0000-0000000000d1';
reset role;
select is((select count(*)::int from public.project_topic_tags where project_id = '00000000-0000-0000-0000-0000000000d1')::int, 1, 'another supervisor cannot remove a project''s tags');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
select throws_ok($$ insert into public.project_topic_tags (project_id, topic_id) values ('00000000-0000-0000-0000-0000000000d1', public.get_or_create_topic('Robotics')) $$, '42501', null, 'a student cannot tag a project');
select is((select count(*)::int from public.project_topic_tags)::int, 1, 'a student can read project tags');
reset role;
set local role anon;
select is((select count(*)::int from public.project_topic_tags)::int, 0, 'an anonymous visitor sees no project tags');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
delete from public.project_topic_tags where project_id = '00000000-0000-0000-0000-0000000000d1';
reset role;
select is((select count(*)::int from public.project_topic_tags where project_id = '00000000-0000-0000-0000-0000000000d1')::int, 0, 'a supervisor can remove their own project''s tags');

-- supervisor interests
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
insert into public.supervisor_topic_interests (supervisor_id, topic_id) values ('00000000-0000-0000-0000-0000000000b1', public.get_or_create_topic('NLP'));
select throws_ok($$ insert into public.supervisor_topic_interests (supervisor_id, topic_id) values ('00000000-0000-0000-0000-0000000000b1', public.get_or_create_topic('NLP')) $$, '23505', null, 'adding the same interest twice is rejected');
select throws_ok($$ insert into public.supervisor_topic_interests (supervisor_id, topic_id) values ('00000000-0000-0000-0000-0000000000b2', public.get_or_create_topic('NLP')) $$, '42501', null, 'a supervisor cannot add another supervisor''s interest');
reset role;
select is((select count(*)::int from public.supervisor_topic_interests where supervisor_id = '00000000-0000-0000-0000-0000000000b1')::int, 1, 'a supervisor can add their own topic interest');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
select is((select count(*)::int from public.supervisor_topic_interests)::int, 1, 'a student can read supervisors'' interests');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b2","role":"authenticated"}';
delete from public.supervisor_topic_interests where supervisor_id = '00000000-0000-0000-0000-0000000000b1';
reset role;
select is((select count(*)::int from public.supervisor_topic_interests where supervisor_id = '00000000-0000-0000-0000-0000000000b1')::int, 1, 'another supervisor cannot remove someone''s interest');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
delete from public.supervisor_topic_interests where supervisor_id = '00000000-0000-0000-0000-0000000000b1';
reset role;
select is((select count(*)::int from public.supervisor_topic_interests where supervisor_id = '00000000-0000-0000-0000-0000000000b1')::int, 0, 'a supervisor can remove their own interest');

-- student interests
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
insert into public.student_topic_interests (student_id, topic_id) values ('00000000-0000-0000-0000-0000000000a1', public.get_or_create_topic('NLP'));
select throws_ok($$ insert into public.student_topic_interests (student_id, topic_id) values ('00000000-0000-0000-0000-0000000000a1', public.get_or_create_topic('NLP')) $$, '23505', null, 'adding the same student interest twice is rejected');
select throws_ok($$ insert into public.student_topic_interests (student_id, topic_id) values ('00000000-0000-0000-0000-0000000000a2', public.get_or_create_topic('NLP')) $$, '42501', null, 'a student cannot add another student''s interest');
reset role;
select is((select count(*)::int from public.student_topic_interests where student_id = '00000000-0000-0000-0000-0000000000a1')::int, 1, 'a student can add their own topic interest');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a2","role":"authenticated"}';
select is((select count(*)::int from public.student_topic_interests)::int, 0, 'a student cannot read another student''s interests');
delete from public.student_topic_interests where student_id = '00000000-0000-0000-0000-0000000000a1';
reset role;
select is((select count(*)::int from public.student_topic_interests where student_id = '00000000-0000-0000-0000-0000000000a1')::int, 1, 'a student cannot remove another student''s interest');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
select is((select count(*)::int from public.student_topic_interests)::int, 0, 'a supervisor cannot read a student''s interests');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000c1","role":"authenticated"}';
select is((select count(*)::int from public.student_topic_interests)::int, 1, 'an admin can read student interests');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
delete from public.student_topic_interests where student_id = '00000000-0000-0000-0000-0000000000a1';
reset role;
select is((select count(*)::int from public.student_topic_interests where student_id = '00000000-0000-0000-0000-0000000000a1')::int, 0, 'a student can remove their own interest');

-- deleting a topic or a project removes the links
insert into public.project_topic_tags (project_id, topic_id) values ('00000000-0000-0000-0000-0000000000d1', public.get_or_create_topic('NLP'));
insert into public.supervisor_topic_interests (supervisor_id, topic_id) values ('00000000-0000-0000-0000-0000000000b1', public.get_or_create_topic('NLP'));
insert into public.student_topic_interests (student_id, topic_id) values ('00000000-0000-0000-0000-0000000000a1', public.get_or_create_topic('NLP'));
delete from public.topics where lower(name) = 'nlp';
select is((select (select count(*) from public.project_topic_tags) + (select count(*) from public.supervisor_topic_interests) + (select count(*) from public.student_topic_interests))::int, 0, 'deleting a topic removes every link to it');
insert into public.project_topic_tags (project_id, topic_id) values ('00000000-0000-0000-0000-0000000000d1', public.get_or_create_topic('Robotics'));
delete from public.projects where id = '00000000-0000-0000-0000-0000000000d1';
select is((select count(*)::int from public.project_topic_tags)::int, 0, 'deleting a project removes its tags');

select * from finish();
rollback;
