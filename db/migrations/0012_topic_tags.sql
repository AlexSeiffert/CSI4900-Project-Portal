-- 0012: the fixed list of topic tags
--
-- Ten broad tags that between them describe almost any project. What each one covers is shown
-- in the app (frontend/web/Components/TopicPalette.cs), along with its colour.
--
-- | tag                 | covers                                                                     |
-- | ------------------- | -------------------------------------------------------------------------- |
-- | AI/ML               | machine learning, deep learning, NLP, computer vision, LLMs, RL, data mining |
-- | Software Engineering| software design, testing and verification, DevOps, languages, compilers, tools |
-- | Web & Mobile        | web apps, mobile apps, full-stack systems, APIs, cross-platform            |
-- | Security & Privacy  | cybersecurity, cryptography, network security, privacy, blockchain         |
-- | Data & Databases    | databases, big data, data engineering, analytics, visualization, IR        |
-- | Systems & Networks  | OS, distributed and cloud, networking, embedded, IoT, HPC                  |
-- | HCI & Design        | UX, accessibility, usability studies, human-centred design, VR/AR          |
-- | Games & Graphics    | game development, graphics, simulation, animation, multimedia              |
-- | Theory & Algorithms | algorithms, complexity, formal methods, computational math, quantum, bioinformatics |
-- | Other               | education tech, robotics, ethics and society, anything else                |
--
-- Topics created before this migration are folded into the matching tag (links are kept and
-- duplicates dropped); anything without a match goes to Other.

do $$
declare
  tags constant text[] := array[
    'AI/ML', 'Software Engineering', 'Web & Mobile', 'Security & Privacy', 'Data & Databases',
    'Systems & Networks', 'HCI & Design', 'Games & Graphics', 'Theory & Algorithms', 'Other'];
  -- earlier topic name (lower case) -> tag
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

    delete from public.topics where id = old.id; -- its links go with it (on delete cascade)
  end loop;
end;
$$;
