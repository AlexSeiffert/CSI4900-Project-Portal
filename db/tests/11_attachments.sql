-- Attachments: the metadata rows, the storage bucket, and who can touch the files.
begin;
select plan(30);

insert into auth.users (id, email, raw_user_meta_data) values
  ('00000000-0000-0000-0000-0000000000a1', 's1@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000a2', 's2@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000a3', 's3@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000b1', 'p1@t.dev', '{"role":"supervisor"}'),
  ('00000000-0000-0000-0000-0000000000b2', 'p2@t.dev', '{"role":"supervisor"}'),
  ('00000000-0000-0000-0000-0000000000c1', 'adm@t.dev', '{"role":"admin"}');
insert into public.inquiries (id, student_id, supervisor_id) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000b1'), ('00000000-0000-0000-0000-0000000000e2', '00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000b2');
insert into public.inquiry_members (inquiry_id, student_id) values
  ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a1'),
  ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a3'),
  ('00000000-0000-0000-0000-0000000000e2', '00000000-0000-0000-0000-0000000000a2');

-- the bucket
select is((select public::text||':'||file_size_limit||':'||array_to_string(allowed_mime_types, ',') from storage.buckets where id = 'inquiry-attachments'), 'false:2097152:application/pdf', 'the bucket is private, PDF only and limited to 2 MB');

-- attachment rows
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
insert into public.inquiry_attachments (inquiry_id, student_id, kind, storage_path, mime_type, size_bytes) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a1', 'resume', '00000000-0000-0000-0000-0000000000a1/00000000-0000-0000-0000-0000000000e1/cv.pdf', 'application/pdf', 1000);
select throws_ok($$ insert into public.inquiry_attachments (inquiry_id, student_id, kind, storage_path, mime_type, size_bytes) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a1', 'resume', '00000000-0000-0000-0000-0000000000a3/00000000-0000-0000-0000-0000000000e1/cv.pdf', 'application/pdf', 1000) $$, '42501', null, 'a path in another member''s folder is rejected');
select throws_ok($$ insert into public.inquiry_attachments (inquiry_id, student_id, kind, storage_path, mime_type, size_bytes) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a3', 'resume', '00000000-0000-0000-0000-0000000000a1/00000000-0000-0000-0000-0000000000e1/x.pdf', 'application/pdf', 1000) $$, '42501', null, 'uploading in another member''s name is rejected');
select throws_ok($$ insert into public.inquiry_attachments (inquiry_id, student_id, kind, storage_path, mime_type, size_bytes) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a1', 'resume', '00000000-0000-0000-0000-0000000000a1/00000000-0000-0000-0000-0000000000e2/cv.pdf', 'application/pdf', 1000) $$, '42501', null, 'a path for a different inquiry is rejected');
select throws_ok($$ insert into public.inquiry_attachments (inquiry_id, student_id, kind, storage_path, mime_type, size_bytes) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a1', 'resume', '00000000-0000-0000-0000-0000000000a1/00000000-0000-0000-0000-0000000000e1/big.pdf', 'application/pdf', 3000000) $$, '23514', null, 'a file over 2 MB is rejected');
select throws_ok($$ insert into public.inquiry_attachments (inquiry_id, student_id, kind, storage_path, mime_type, size_bytes) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a1', 'resume', '00000000-0000-0000-0000-0000000000a1/00000000-0000-0000-0000-0000000000e1/empty.pdf', 'application/pdf', 0) $$, '23514', null, 'an empty file is rejected');
select throws_ok($$ insert into public.inquiry_attachments (inquiry_id, student_id, kind, storage_path, mime_type, size_bytes) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a1', 'resume', '00000000-0000-0000-0000-0000000000a1/00000000-0000-0000-0000-0000000000e1/cv.pdf', 'application/pdf', 1000) $$, '23505', null, 'the same file path twice is rejected');
select throws_ok($$ insert into public.inquiry_attachments (inquiry_id, student_id, kind, storage_path, mime_type, size_bytes) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a1', 'portfolio', '00000000-0000-0000-0000-0000000000a1/00000000-0000-0000-0000-0000000000e1/p.pdf', 'application/pdf', 1000) $$, '22P02', null, 'a kind other than resume or transcript is rejected');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a3","role":"authenticated"}';
insert into public.inquiry_attachments (inquiry_id, student_id, kind, storage_path, mime_type, size_bytes) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a3', 'transcript', '00000000-0000-0000-0000-0000000000a3/00000000-0000-0000-0000-0000000000e1/t.pdf', 'application/pdf', 1000);
reset role;
select is((select count(*)::int from public.inquiry_attachments where inquiry_id = '00000000-0000-0000-0000-0000000000e1')::int, 2, 'the creator and a friend can each attach their own files');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a2","role":"authenticated"}';
select throws_ok($$ insert into public.inquiry_attachments (inquiry_id, student_id, kind, storage_path, mime_type, size_bytes) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a2', 'resume', '00000000-0000-0000-0000-0000000000a2/00000000-0000-0000-0000-0000000000e1/cv.pdf', 'application/pdf', 1000) $$, '42501', null, 'a non-member cannot attach to an inquiry');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
select throws_ok($$ insert into public.inquiry_attachments (inquiry_id, student_id, kind, storage_path, mime_type, size_bytes) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000b1', 'resume', '00000000-0000-0000-0000-0000000000b1/00000000-0000-0000-0000-0000000000e1/cv.pdf', 'application/pdf', 1000) $$, '42501', null, 'a supervisor cannot attach files');
reset role;

-- who can see the rows
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a3","role":"authenticated"}';
select is((select count(*)::int from public.inquiry_attachments)::int, 2, 'a member sees every member''s attachment rows');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
select is((select count(*)::int from public.inquiry_attachments)::int, 2, 'the supervisor on the inquiry sees them all');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a2","role":"authenticated"}';
select is((select count(*)::int from public.inquiry_attachments)::int, 0, 'another student does not');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b2","role":"authenticated"}';
select is((select count(*)::int from public.inquiry_attachments)::int, 0, 'another supervisor does not');
reset role;
set local role anon;
select is((select count(*)::int from public.inquiry_attachments)::int, 0, 'an anonymous visitor does not');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
delete from public.inquiry_attachments where inquiry_id = '00000000-0000-0000-0000-0000000000e1';
reset role;
select is((select count(*)::int from public.inquiry_attachments)::int, 2, 'a supervisor cannot remove an attachment row');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
delete from public.inquiry_attachments where inquiry_id = '00000000-0000-0000-0000-0000000000e1';
reset role;
select is((select count(*)::int from public.inquiry_attachments)::int, 1, 'a member removes only their own attachment rows');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a3","role":"authenticated"}';
delete from public.inquiry_members where inquiry_id = '00000000-0000-0000-0000-0000000000e1' and student_id = '00000000-0000-0000-0000-0000000000a3';
reset role;
select is((select count(*)::int from public.inquiry_attachments)::int, 0, 'a member who leaves takes their attachment rows with them');
insert into public.inquiry_attachments (inquiry_id, student_id, kind, storage_path, mime_type, size_bytes) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a1', 'resume', '00000000-0000-0000-0000-0000000000a1/00000000-0000-0000-0000-0000000000e1/cv.pdf', 'application/pdf', 1000);
delete from public.inquiries where id = '00000000-0000-0000-0000-0000000000e1';
select is((select count(*)::int from public.inquiry_attachments)::int, 0, 'deleting an inquiry deletes its attachment rows');
insert into public.inquiries (id, student_id, supervisor_id) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000b1');
insert into public.inquiry_members (inquiry_id, student_id) values ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a1'), ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a3');

-- the files in storage
-- Storage refuses direct deletes unless this switch is set, as its own API does
set local storage.allow_delete_query = 'true';
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
insert into storage.objects (bucket_id, name) values ('inquiry-attachments', '00000000-0000-0000-0000-0000000000a1/00000000-0000-0000-0000-0000000000e1/cv.pdf');
select throws_ok($$ insert into storage.objects (bucket_id, name) values ('inquiry-attachments', '00000000-0000-0000-0000-0000000000a3/00000000-0000-0000-0000-0000000000e1/cv.pdf') $$, '42501', null, 'a member cannot upload into another member''s folder');
select throws_ok($$ insert into storage.objects (bucket_id, name) values ('inquiry-attachments', '00000000-0000-0000-0000-0000000000a1/00000000-0000-0000-0000-0000000000e2/cv.pdf') $$, '42501', null, 'a student cannot upload to an inquiry they are not on');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a3","role":"authenticated"}';
insert into storage.objects (bucket_id, name) values ('inquiry-attachments', '00000000-0000-0000-0000-0000000000a3/00000000-0000-0000-0000-0000000000e1/t.pdf');
reset role;
select is((select count(*)::int from storage.objects where bucket_id = 'inquiry-attachments')::int, 2, 'the creator and a friend can each upload into their own folder');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
select is((select count(*)::int from storage.objects where bucket_id = 'inquiry-attachments')::int, 2, 'a member can read every member''s files');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
select is((select count(*)::int from storage.objects where bucket_id = 'inquiry-attachments')::int, 2, 'the supervisor on the inquiry can read them');
delete from storage.objects where bucket_id = 'inquiry-attachments';
reset role;
select is((select count(*)::int from storage.objects where bucket_id = 'inquiry-attachments')::int, 2, 'the supervisor cannot delete them');
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a2","role":"authenticated"}';
select is((select count(*)::int from storage.objects where bucket_id = 'inquiry-attachments')::int, 0, 'another student cannot read them');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b2","role":"authenticated"}';
select is((select count(*)::int from storage.objects where bucket_id = 'inquiry-attachments')::int, 0, 'another supervisor cannot read them');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000c1","role":"authenticated"}';
select is((select count(*)::int from storage.objects where bucket_id = 'inquiry-attachments')::int, 2, 'an admin can read them');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
delete from storage.objects where bucket_id = 'inquiry-attachments';
reset role;
select is((select count(*)::int from storage.objects where bucket_id = 'inquiry-attachments')::int, 1, 'a member can delete only their own file');

select * from finish();
rollback;
