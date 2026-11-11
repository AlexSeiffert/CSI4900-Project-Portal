do $$
declare
  tags constant text[] := array[
    'AI/ML', 'Software Engineering', 'Web & Mobile', 'Security & Privacy', 'Data & Databases',
    'Systems & Networks', 'HCI & Design', 'Games & Graphics', 'Theory & Algorithms', 'Other'];
  renames constant jsonb := '{
    "artificial intelligence": "AI/ML", "machine learning": "AI/ML", "ai": "AI/ML",
    "software engineering": "Software Engineering",
    "web development": "Web & Mobile",
    "cybersecurity": "Security & Privacy", "security": "Security & Privacy",
    "databases": "Data & Databases", "data science": "Data & Databases",
    "distributed systems": "Systems & Networks", "networking": "Systems & Networks",
    "human-computer interaction": "HCI & Design", "accessibility": "HCI & Design",
    "computer graphics": "Games & Graphics", "game development": "Games & Graphics",
    "algorithms": "Theory & Algorithms"
  }';
  tag text;
  old record;
  target uuid;
begin
  foreach tag in array tags loop
    insert into public.topics (name) values (tag) on conflict ((lower(name))) do nothing;
  end loop;

  for old in select id, name from public.topics where name <> all (tags) loop
    select id into target from public.topics
    where name = coalesce(renames ->> lower(old.name), 'Other');

    insert into public.project_topic_tags (project_id, topic_id)
      select project_id, target from public.project_topic_tags where topic_id = old.id
      on conflict do nothing;
    insert into public.student_topic_interests (student_id, topic_id)
      select student_id, target from public.student_topic_interests where topic_id = old.id
      on conflict do nothing;
    insert into public.supervisor_topic_interests (supervisor_id, topic_id)
      select supervisor_id, target from public.supervisor_topic_interests where topic_id = old.id
      on conflict do nothing;

    delete from public.topics where id = old.id;
  end loop;
end;
$$;
