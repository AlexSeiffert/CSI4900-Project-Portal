-- Inquiries: groups of members, who can see them, and how the status moves.
begin;
select plan(61);

insert into auth.users (id, email, raw_user_meta_data) values
  ('00000000-0000-0000-0000-0000000000a1', 's1@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000a2', 's2@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000a3', 's3@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000a4', 's4@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000b1', 'p1@t.dev', '{"role":"supervisor"}'),
  ('00000000-0000-0000-0000-0000000000b2', 'p2@t.dev', '{"role":"supervisor"}'),
  ('00000000-0000-0000-0000-0000000000c1', 'adm@t.dev', '{"role":"admin"}');
insert into public.projects (id, supervisor_id, title, description) values ('00000000-0000-0000-0000-0000000000d1', '00000000-0000-0000-0000-0000000000b1', 'T', 'D'), ('00000000-0000-0000-0000-0000000000d2', '00000000-0000-0000-0000-0000000000b2', 'Other', 'D');

-- a student creates a request and builds the group
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
insert into public.inquiries (id, student_id, supervisor_id, project_id, message) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000b1', '00000000-0000-0000-0000-0000000000d1', 'Hello');
insert into public.inquiries (id, student_id, supervisor_id) values ('00000000-0000-0000-0000-0000000000e2', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000b1');
insert into public.inquiry_members (inquiry_id, student_id) values
  ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a1'),
  ('00000000-0000-0000-0000-0000000000e1', public.find_student_by_email('S2@t.dev ')),
  ('00000000-0000-0000-0000-0000000000e1', public.find_student_by_email('s3@t.dev')),
  ('00000000-0000-0000-0000-0000000000e2', '00000000-0000-0000-0000-0000000000a1');
select throws_ok($$ insert into public.inquiries (student_id, supervisor_id, project_id) values ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000b1', '00000000-0000-0000-0000-0000000000d2') $$, '42501', null, 'an inquiry about another supervisor''s project is rejected');
select throws_ok($$ insert into public.inquiries (student_id, supervisor_id, status) values ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000b1', 'sent') $$, '42501', null, 'an inquiry cannot start as anything but a draft');
select throws_ok($$ insert into public.inquiries (student_id, supervisor_id) values ('00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000b1') $$, '42501', null, 'an inquiry on behalf of another student is rejected');
select throws_ok($$ insert into public.inquiries (student_id, supervisor_id) values ('00000000-0000-0000-0000-0000000000a1', gen_random_uuid()) $$, '23503', null, 'an inquiry to a supervisor that does not exist is rejected');
select throws_ok($$ insert into public.inquiries (student_id, supervisor_id, message) values ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000b1', repeat('x', 2001)) $$, '23514', null, 'a message over 2000 characters is rejected');
select throws_ok($$ insert into public.inquiry_members (inquiry_id, student_id) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a4') $$, '23514', null, 'a fourth member is rejected');
select throws_ok($$ insert into public.inquiry_members (inquiry_id, student_id) values ('00000000-0000-0000-0000-0000000000e2', '00000000-0000-0000-0000-0000000000a1') $$, '23505', null, 'the same member twice is rejected');
select throws_ok($$ insert into public.inquiry_members (inquiry_id, student_id, response) values ('00000000-0000-0000-0000-0000000000e2', '00000000-0000-0000-0000-0000000000a4', 'accepted') $$, '42501', null, 'a new member cannot start as accepted');
select throws_ok($$ insert into public.inquiry_members (inquiry_id, student_id) values ('00000000-0000-0000-0000-0000000000e2', '00000000-0000-0000-0000-0000000000b1') $$, '23503', null, 'a supervisor cannot be added as a member');
select throws_ok($$ select public.find_student_by_email('nobody@t.dev') $$, 'P0002', 'not found', 'an unregistered email is not found');
select throws_ok($$ select public.find_student_by_email('p1@t.dev') $$, 'P0002', 'not found', 'a supervisor''s email is not found as a student');
select is((select count(*)::int from public.users)::int, 1, 'the creator still cannot read other users');
select is((select count(*)::int from public.inquiry_member_emails('00000000-0000-0000-0000-0000000000e1'))::int, 3, 'a member gets the group''s emails for the CC line');
reset role;
select is((select status::text from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1'), 'draft', 'a new inquiry starts as a draft');
select is((select count(*)::int from public.inquiry_members where inquiry_id = '00000000-0000-0000-0000-0000000000e1' and response = 'pending')::int, 3, 'the creator can add two friends by email, all pending');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a2","role":"authenticated"}';
select throws_ok($$ insert into public.inquiry_members (inquiry_id, student_id) values ('00000000-0000-0000-0000-0000000000e2', '00000000-0000-0000-0000-0000000000a2') $$, '42501', null, 'only the creator can add members');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
select throws_ok($$ insert into public.inquiries (student_id, supervisor_id) values ('00000000-0000-0000-0000-0000000000b1', '00000000-0000-0000-0000-0000000000b2') $$, '23503', null, 'a supervisor cannot create an inquiry');
reset role;
set local role anon;
select throws_ok($$ select public.find_student_by_email('s1@t.dev') $$, '42501', null, 'an anonymous visitor cannot look up emails');
reset role;

-- who can see them
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a2","role":"authenticated"}';
select is((select count(*)::int from public.inquiries)::int, 1, 'a friend sees the inquiry they were added to');
select is((select count(*)::int from public.inquiry_members where inquiry_id = '00000000-0000-0000-0000-0000000000e1')::int, 3, 'and its members');
select is((select count(*)::int from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a3')::int, 1, 'and the other members'' profiles');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a4","role":"authenticated"}';
select is((select count(*)::int from public.inquiries)::int, 0, 'a non-member cannot read an inquiry');
select is((select count(*)::int from public.inquiry_members)::int, 0, 'or its members');
select is((select count(*)::int from public.inquiry_member_emails('00000000-0000-0000-0000-0000000000e1'))::int, 0, 'or their emails');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
select is((select count(*)::int from public.inquiries)::int, 2, 'a supervisor sees the inquiries addressed to them');
select is((select count(*)::int from public.student_profiles where user_id in ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000a3'))::int, 3, 'and the profile of every member');
select is((select count(*)::int from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a4')::int, 0, 'but not of a student who is not on one');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b2","role":"authenticated"}';
select is((select count(*)::int from public.inquiries)::int, 0, 'an uninvolved supervisor sees no inquiries');
select is((select count(*)::int from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a1')::int, 0, 'and cannot read a member''s profile');
reset role;
set local role anon;
select is((select count(*)::int from public.inquiries)::int, 0, 'an anonymous visitor sees no inquiries');
reset role;

-- sending and offering
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
select throws_ok($$ update public.inquiries set status = 'offered' where id = '00000000-0000-0000-0000-0000000000e1' $$, '42501', null, 'a supervisor cannot offer on a draft');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a2","role":"authenticated"}';
select throws_ok($$ update public.inquiries set status = 'matched' where id = '00000000-0000-0000-0000-0000000000e1' $$, '42501', null, 'a student cannot jump from draft to matched');
update public.inquiries set status = 'sent' where id = '00000000-0000-0000-0000-0000000000e1';
reset role;
select is((select status::text from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1'), 'sent', 'any member can mark it sent');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
select throws_ok($$ update public.inquiries set status = 'offered' where id = '00000000-0000-0000-0000-0000000000e1' $$, '42501', null, 'a student cannot make the offer');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
select throws_ok($$ update public.inquiries set status = 'matched' where id = '00000000-0000-0000-0000-0000000000e1' $$, '42501', null, 'a supervisor cannot set matched');
select throws_ok($$ update public.inquiries set message = 'x' where id = '00000000-0000-0000-0000-0000000000e1' $$, '42501', null, 'a supervisor cannot change the message');
update public.inquiries set status = 'offered' where id = '00000000-0000-0000-0000-0000000000e1';
reset role;
select is((select status::text from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1'), 'offered', 'the supervisor can offer a sent inquiry');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
select throws_ok($$ update public.inquiries set supervisor_id = '00000000-0000-0000-0000-0000000000b2' where id = '00000000-0000-0000-0000-0000000000e1' $$, '42501', null, 'an inquiry cannot be sent on to another supervisor');
select throws_ok($$ update public.inquiries set student_id = '00000000-0000-0000-0000-0000000000a2' where id = '00000000-0000-0000-0000-0000000000e1' $$, '42501', null, 'an inquiry cannot be given to another student');
select throws_ok($$ update public.inquiries set project_id = '00000000-0000-0000-0000-0000000000d2' where id = '00000000-0000-0000-0000-0000000000e2' $$, '42501', null, 'an inquiry cannot be linked to a project afterwards');

-- the members answer the offer
select throws_ok($$ update public.inquiries set status = 'matched' where id = '00000000-0000-0000-0000-0000000000e1' $$, '42501', null, 'it cannot be matched while members are still pending');
update public.inquiry_members set response = 'accepted' where inquiry_id = '00000000-0000-0000-0000-0000000000e1' and student_id = '00000000-0000-0000-0000-0000000000a2';
update public.inquiry_members set response = 'accepted' where inquiry_id = '00000000-0000-0000-0000-0000000000e1' and student_id = '00000000-0000-0000-0000-0000000000a1';
select throws_ok($$ update public.inquiry_members set response = 'declined' where inquiry_id = '00000000-0000-0000-0000-0000000000e1' and student_id = '00000000-0000-0000-0000-0000000000a1' $$, '42501', null, 'a member cannot change their answer');
reset role;
select is((select response::text from public.inquiry_members where inquiry_id = '00000000-0000-0000-0000-0000000000e1' and student_id = '00000000-0000-0000-0000-0000000000a2'), 'pending', 'a member''s answer is theirs alone');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a2","role":"authenticated"}';
update public.inquiry_members set response = 'declined' where inquiry_id = '00000000-0000-0000-0000-0000000000e1' and student_id = '00000000-0000-0000-0000-0000000000a2';
delete from public.inquiry_members where inquiry_id = '00000000-0000-0000-0000-0000000000e1' and student_id = '00000000-0000-0000-0000-0000000000a2';
select is((select count(*)::int from public.inquiries)::int, 0, 'a member who declines leaves the inquiry');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a3","role":"authenticated"}';
update public.inquiry_members set response = 'accepted' where inquiry_id = '00000000-0000-0000-0000-0000000000e1' and student_id = '00000000-0000-0000-0000-0000000000a3';
update public.inquiries set status = 'matched' where id = '00000000-0000-0000-0000-0000000000e1';
reset role;
select is((select status::text from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1'), 'matched', 'once nobody is pending, a member who accepted confirms the match');
select is((select count(*)::int from public.inquiry_members where inquiry_id = '00000000-0000-0000-0000-0000000000e1')::int, 2, 'with the two who accepted');

-- everyone declines
insert into public.inquiries (id, student_id, supervisor_id, status) values ('00000000-0000-0000-0000-0000000000e3', '00000000-0000-0000-0000-0000000000a4', '00000000-0000-0000-0000-0000000000b2', 'offered');
insert into public.inquiry_members (inquiry_id, student_id) values ('00000000-0000-0000-0000-0000000000e3', '00000000-0000-0000-0000-0000000000a4');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a4","role":"authenticated"}';
update public.inquiry_members set response = 'declined' where inquiry_id = '00000000-0000-0000-0000-0000000000e3';
select throws_ok($$ update public.inquiries set status = 'matched' where id = '00000000-0000-0000-0000-0000000000e3' $$, '42501', null, 'it cannot be matched when nobody accepted');
update public.inquiries set status = 'closed' where id = '00000000-0000-0000-0000-0000000000e3';
delete from public.inquiry_members where inquiry_id = '00000000-0000-0000-0000-0000000000e3';
select is((select count(*)::int from public.inquiries)::int, 1, 'a creator who declined still sees the request they created');
reset role;
select is((select status::text from public.inquiries where id = '00000000-0000-0000-0000-0000000000e3'), 'closed', 'when everyone declines it closes');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b2","role":"authenticated"}';
select is((select count(*)::int from public.student_profiles where user_id = '00000000-0000-0000-0000-0000000000a4')::int, 1, 'and the supervisor can still read the creator''s profile');
reset role;

-- closing and removing members
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
insert into public.inquiry_members (inquiry_id, student_id) values ('00000000-0000-0000-0000-0000000000e2', '00000000-0000-0000-0000-0000000000a4');
delete from public.inquiry_members where inquiry_id = '00000000-0000-0000-0000-0000000000e2' and student_id = '00000000-0000-0000-0000-0000000000a4';
select is((select count(*)::int from public.inquiry_members where inquiry_id = '00000000-0000-0000-0000-0000000000e2')::int, 1, 'the creator can remove a friend from a draft');
reset role;
update public.inquiries set updated_at = now() - interval '1 day' where id = '00000000-0000-0000-0000-0000000000e2';
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a4","role":"authenticated"}';
update public.inquiries set status = 'closed' where id = '00000000-0000-0000-0000-0000000000e2';
reset role;
select is((select status::text from public.inquiries where id = '00000000-0000-0000-0000-0000000000e2'), 'draft', 'an uninvolved student cannot change an inquiry');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
update public.inquiries set status = 'closed' where id = '00000000-0000-0000-0000-0000000000e2';
reset role;
select is((select status::text from public.inquiries where id = '00000000-0000-0000-0000-0000000000e2'), 'closed', 'the supervisor can close an inquiry at any time');
select is((select updated_at > now() - interval '1 hour' from public.inquiries where id = '00000000-0000-0000-0000-0000000000e2'), true, 'changing the status refreshes updated_at');

-- deleting
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
delete from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1';
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
delete from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1';
reset role;
select is((select count(*)::int from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1')::int, 1, 'neither a student nor a supervisor can delete an inquiry');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000c1","role":"authenticated"}';
select is((select count(*)::int from public.inquiries)::int, 3, 'an admin sees every inquiry');
update public.inquiries set message = 'edited' where id = '00000000-0000-0000-0000-0000000000e1';
reset role;
select is((select message from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1'), 'edited', 'an admin can edit an inquiry');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
delete from public.projects where id = '00000000-0000-0000-0000-0000000000d1';
reset role;
select is((select count(*)::int from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1' and project_id is null)::int, 1, 'deleting a project keeps its inquiries, with no project');
delete from public.users where id = '00000000-0000-0000-0000-0000000000a3';
select is((select count(*)::int from public.inquiry_members where student_id = '00000000-0000-0000-0000-0000000000a3')::int, 0, 'deleting a student removes their memberships');
delete from public.users where id = '00000000-0000-0000-0000-0000000000a1';
select is((select count(*)::int from public.inquiries where student_id = '00000000-0000-0000-0000-0000000000a1')::int, 0, 'deleting a student deletes the inquiries they created');
delete from public.users where id = '00000000-0000-0000-0000-0000000000b2';
select is((select count(*)::int from public.inquiries where id = '00000000-0000-0000-0000-0000000000e3')::int, 0, 'deleting a supervisor deletes the inquiries sent to them');

select * from finish();
rollback;
