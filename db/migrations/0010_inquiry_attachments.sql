-- 0010: files a student attaches to an inquiry (a CV, a transcript)
--
-- The file itself lives in Supabase Storage. Postgres only holds a row describing it.
--
-- | column       | type    | notes                                                       |
-- | ------------ | ------- | ----------------------------------------------------------- |
-- | id           | uuid    | PK                                                          |
-- | inquiry_id   | uuid    | FK -> inquiries(id)                                         |
-- | storage_path | text    | the file's name inside the bucket, unique                   |
-- | mime_type    | text    |                                                             |
-- | size_bytes   | integer | 1 byte to 2 MB                                              |
-- | created_at   | timestamptz |                                                         |
--
-- Bucket: inquiry-attachments (private, PDF only, 2 MB per file, enforced by Storage itself).
-- A file's path inside the bucket is <student_id>/<inquiry_id>/<file_name>.
--
-- Intended upload flow: the browser asks the API for a signed upload URL, uploads the file
-- straight to Storage, then tells the API the path, and the API inserts the row here.
--
-- Only the student and the supervisor on the inquiry can read the row or the file. Only the
-- student can add or remove one. Deleting an inquiry deletes its rows here, but NOT the files
-- in Storage: those become orphans and are cleaned up by hand for now.

create table public.inquiry_attachments (
  id uuid primary key default gen_random_uuid(),
  inquiry_id uuid not null references public.inquiries (id) on delete cascade,
  storage_path text not null unique,
  mime_type text not null,
  size_bytes integer not null check (size_bytes > 0 and size_bytes <= 2097152),
  created_at timestamptz not null default now()
);

create index inquiry_attachments_inquiry_id_idx on public.inquiry_attachments (inquiry_id);

alter table public.inquiry_attachments enable row level security;

create policy inquiry_attachments_select_involved
  on public.inquiry_attachments
  for select
  to authenticated
  using (
    exists (
      select 1 from public.inquiries i
      where i.id = inquiry_attachments.inquiry_id
        and (i.student_id = auth.uid() or i.supervisor_id = auth.uid())
    )
  );

create policy inquiry_attachments_insert_own
  on public.inquiry_attachments
  for insert
  to authenticated
  with check (
    exists (
      select 1 from public.inquiries i
      where i.id = inquiry_attachments.inquiry_id and i.student_id = auth.uid()
    )
    and inquiry_attachments.storage_path like auth.uid()::text || '/' || inquiry_attachments.inquiry_id::text || '/%'
  );

create policy inquiry_attachments_delete_own
  on public.inquiry_attachments
  for delete
  to authenticated
  using (
    exists (
      select 1 from public.inquiries i
      where i.id = inquiry_attachments.inquiry_id and i.student_id = auth.uid()
    )
  );

create policy inquiry_attachments_admin_full_access
  on public.inquiry_attachments
  for all
  to authenticated
  using (public.current_user_role() = 'admin')
  with check (public.current_user_role() = 'admin');

-- The Storage bucket: file_size_limit and allowed_mime_types are enforced by Storage on upload.
insert into storage.buckets (id, name, public, file_size_limit, allowed_mime_types)
values ('inquiry-attachments', 'inquiry-attachments', false, 2097152, array['application/pdf'])
on conflict (id) do update
  set public = excluded.public,
      file_size_limit = excluded.file_size_limit,
      allowed_mime_types = excluded.allowed_mime_types;

create policy inquiry_attachments_files_upload
  on storage.objects
  for insert
  to authenticated
  with check (
    bucket_id = 'inquiry-attachments'
    and (storage.foldername(name))[1] = auth.uid()::text
    and exists (
      select 1 from public.inquiries i
      where i.id::text = (storage.foldername(name))[2] and i.student_id = auth.uid()
    )
  );

create policy inquiry_attachments_files_read
  on storage.objects
  for select
  to authenticated
  using (
    bucket_id = 'inquiry-attachments'
    and exists (
      select 1 from public.inquiries i
      where i.id::text = (storage.foldername(name))[2]
        and (i.student_id = auth.uid() or i.supervisor_id = auth.uid())
    )
  );

create policy inquiry_attachments_files_delete
  on storage.objects
  for delete
  to authenticated
  using (
    bucket_id = 'inquiry-attachments'
    and (storage.foldername(name))[1] = auth.uid()::text
  );

create policy inquiry_attachments_files_admin_full_access
  on storage.objects
  for all
  to authenticated
  using (bucket_id = 'inquiry-attachments' and public.current_user_role() = 'admin')
  with check (bucket_id = 'inquiry-attachments' and public.current_user_role() = 'admin');
