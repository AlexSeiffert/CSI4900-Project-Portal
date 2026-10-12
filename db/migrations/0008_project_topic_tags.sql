-- 0008: links between topics and supervisors, students and projects
--
-- | table                      | links                         | who can change a row                 |
-- | -------------------------- | ----------------------------- | ------------------------------------ |
-- | supervisor_topic_interests | supervisor profile <-> topic  | that supervisor                      |
-- | student_topic_interests    | student profile <-> topic     | that student                         |
-- | project_topic_tags         | project <-> topic             | the supervisor who owns the project  |
--
-- Add a link with the topic id from get_or_create_topic() (0006). A link has no other
-- data, so it is added or removed, never edited.
--
-- Supervisor interests and project tags are readable by everyone signed in (they are shown
-- on profiles and in browsing). A student's interests are readable only by that student.

create table public.supervisor_topic_interests (
  supervisor_id uuid not null references public.supervisor_profiles (user_id) on delete cascade,
  topic_id uuid not null references public.topics (id) on delete cascade,
  primary key (supervisor_id, topic_id)
);

create table public.student_topic_interests (
  student_id uuid not null references public.student_profiles (user_id) on delete cascade,
  topic_id uuid not null references public.topics (id) on delete cascade,
  primary key (student_id, topic_id)
);

create table public.project_topic_tags (
  project_id uuid not null references public.projects (id) on delete cascade,
  topic_id uuid not null references public.topics (id) on delete cascade,
  primary key (project_id, topic_id)
);

create index supervisor_topic_interests_topic_id_idx on public.supervisor_topic_interests (topic_id);
create index student_topic_interests_topic_id_idx on public.student_topic_interests (topic_id);
create index project_topic_tags_topic_id_idx on public.project_topic_tags (topic_id);

alter table public.supervisor_topic_interests enable row level security;
alter table public.student_topic_interests enable row level security;
alter table public.project_topic_tags enable row level security;

-- supervisor_topic_interests
create policy supervisor_topic_interests_select_all
  on public.supervisor_topic_interests
  for select
  to authenticated
  using (true);

create policy supervisor_topic_interests_insert_own
  on public.supervisor_topic_interests
  for insert
  to authenticated
  with check (supervisor_id = auth.uid());

create policy supervisor_topic_interests_delete_own
  on public.supervisor_topic_interests
  for delete
  to authenticated
  using (supervisor_id = auth.uid());

create policy supervisor_topic_interests_admin_full_access
  on public.supervisor_topic_interests
  for all
  to authenticated
  using (public.current_user_role() = 'admin')
  with check (public.current_user_role() = 'admin');

-- student_topic_interests
create policy student_topic_interests_select_own
  on public.student_topic_interests
  for select
  to authenticated
  using (student_id = auth.uid());

create policy student_topic_interests_insert_own
  on public.student_topic_interests
  for insert
  to authenticated
  with check (student_id = auth.uid());

create policy student_topic_interests_delete_own
  on public.student_topic_interests
  for delete
  to authenticated
  using (student_id = auth.uid());

create policy student_topic_interests_admin_full_access
  on public.student_topic_interests
  for all
  to authenticated
  using (public.current_user_role() = 'admin')
  with check (public.current_user_role() = 'admin');

-- project_topic_tags
create policy project_topic_tags_select_all
  on public.project_topic_tags
  for select
  to authenticated
  using (true);

create policy project_topic_tags_insert_own
  on public.project_topic_tags
  for insert
  to authenticated
  with check (
    exists (
      select 1 from public.projects p
      where p.id = project_topic_tags.project_id and p.supervisor_id = auth.uid()
    )
  );

create policy project_topic_tags_delete_own
  on public.project_topic_tags
  for delete
  to authenticated
  using (
    exists (
      select 1 from public.projects p
      where p.id = project_topic_tags.project_id and p.supervisor_id = auth.uid()
    )
  );

create policy project_topic_tags_admin_full_access
  on public.project_topic_tags
  for all
  to authenticated
  using (public.current_user_role() = 'admin')
  with check (public.current_user_role() = 'admin');
