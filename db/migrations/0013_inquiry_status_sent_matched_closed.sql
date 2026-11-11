drop policy inquiries_insert_own on public.inquiries;

alter table public.inquiries alter column status drop default;
alter type public.inquiry_status rename to inquiry_status_old;
create type public.inquiry_status as enum ('sent', 'matched', 'closed');

alter table public.inquiries
  alter column status type public.inquiry_status
  using (case status::text when 'closed' then 'closed' else 'sent' end)::public.inquiry_status;
alter table public.inquiries alter column status set default 'sent';

drop type public.inquiry_status_old;

create policy inquiries_insert_own
  on public.inquiries
  for insert
  to authenticated
  with check (
    student_id = auth.uid()
    and status = 'sent'
    and (
      inquiries.project_id is null
      or exists (
        select 1 from public.projects p
        where p.id = inquiries.project_id and p.supervisor_id = inquiries.supervisor_id
      )
    )
  );
