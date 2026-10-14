-- Topics: reuse by name ignoring case, and who can add or change them.
begin;
select plan(16);

insert into auth.users (id, email, raw_user_meta_data) values
  ('00000000-0000-0000-0000-0000000000a1', 's1@t.dev', '{"role":"student"}'),
  ('00000000-0000-0000-0000-0000000000b1', 'p1@t.dev', '{"role":"supervisor"}');

-- get_or_create_topic
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
select is((select public.get_or_create_topic('NLP') = public.get_or_create_topic('  nlp ')), true, 'the same topic comes back whatever the case or spacing');
do $$ begin perform public.get_or_create_topic('Robotics'); end $$;
select throws_ok($$ select public.get_or_create_topic('   ') $$, '22023', null, 'a blank topic name is rejected');
select throws_ok($$ select public.get_or_create_topic(null) $$, '22023', null, 'a missing topic name is rejected');

-- writing directly is not allowed
select throws_ok($$ insert into public.topics (name) values ('Direct') $$, '42501', null, 'a student cannot insert topics directly');
update public.topics set name = 'Renamed' where lower(name) = 'nlp';
delete from public.topics where lower(name) = 'robotics';
reset role;
select is((select count(*)::int from public.topics where lower(name) = 'nlp')::int, 1, 'only one row was created for NLP');
select is((select name from public.topics where lower(name) = 'nlp'), 'NLP', 'the name is stored trimmed, as first given');
select is((select count(*)::int from public.topics where name = 'Renamed')::int, 0, 'a student cannot rename a topic');
select is((select count(*)::int from public.topics where lower(name) = 'robotics')::int, 1, 'a student cannot delete a topic');
select is((select public.get_or_create_topic('NLP') <> public.get_or_create_topic('Robotics')), true, 'different names get different topics');

-- who can read
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000a1","role":"authenticated"}';
select is((select count(*) >= 2 from public.topics), true, 'a student can read topics');
reset role;
set local role authenticated;
set local request.jwt.claims = '{"sub":"00000000-0000-0000-0000-0000000000b1","role":"authenticated"}';
select is((select count(*) >= 2 from public.topics), true, 'a supervisor can read topics');
reset role;
set local role anon;
select is((select count(*)::int from public.topics)::int, 0, 'an anonymous visitor sees no topics');
select throws_ok($$ select public.get_or_create_topic('Anon') $$, '42501', null, 'an anonymous visitor cannot create topics');
reset role;

-- constraints
select throws_ok($$ insert into public.topics (name) values ('nlp') $$, '23505', null, 'topic names are unique ignoring case');
select throws_ok($$ insert into public.topics (name) values (' padded ') $$, '23514', null, 'topic names cannot have leading or trailing spaces');
select throws_ok($$ insert into public.topics (name) values ('') $$, '23514', null, 'topic names cannot be empty');

select * from finish();
rollback;
