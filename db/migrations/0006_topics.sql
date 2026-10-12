-- 0006: topics, one shared list of research topics
--
-- Used by supervisor interests, student interests and project tags (see 0008).
--
-- | column | type | notes                                                   |
-- | ------ | ---- | ------------------------------------------------------- |
-- | id     | uuid | PK                                                      |
-- | name   | text | trimmed, not empty, unique ignoring case ("NLP" = "nlp") |
--
-- Everyone signed in can read topics, but nobody inserts into the table directly:
-- new topics go through get_or_create_topic(), which finds an existing topic by name
-- (ignoring case) or creates it, and returns its id.

create table public.topics (
  id uuid primary key default gen_random_uuid(),
  name text not null check (name = btrim(name) and name <> '')
);

create unique index topics_name_lower_key on public.topics (lower(name));

-- SECURITY DEFINER so people can add topics without having write access to the table.
create or replace function public.get_or_create_topic(topic_name text)
returns uuid
language plpgsql
security definer
set search_path = public
as $$
declare
  clean text := btrim(topic_name);
  topic_id uuid;
begin
  if clean is null or clean = '' then
    raise exception 'topic name cannot be empty' using errcode = '22023';
  end if;

  insert into public.topics (name) values (clean)
  on conflict ((lower(name))) do nothing;

  select id into topic_id from public.topics where lower(name) = lower(clean);
  return topic_id;
end;
$$;

revoke execute on function public.get_or_create_topic(text) from public, anon;
grant execute on function public.get_or_create_topic(text) to authenticated;

alter table public.topics enable row level security;

create policy topics_select_all
  on public.topics
  for select
  to authenticated
  using (true);
