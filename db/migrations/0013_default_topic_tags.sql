insert into public.topics (name)
values ('AI/ML'), ('Software Engineering'), ('Web & Mobile'), ('Security & Privacy'),
       ('Data & Databases'), ('Systems & Networks'), ('HCI & Design'), ('Games & Graphics'),
       ('Theory & Algorithms'), ('Other')
on conflict ((lower(name))) do nothing;
