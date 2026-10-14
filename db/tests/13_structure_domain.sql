-- Structure: the tables, keys, triggers, policies and grants migrations 0005-0011 should have created.
begin;
select plan(73);


-- tables, columns and policies
select has_table('public', 'student_profiles', 'public.student_profiles exists');
select columns_are('public', 'student_profiles', array['user_id', 'display_name', 'program', 'updated_at'], 'public.student_profiles has exactly its expected columns');
select policies_are('public', 'student_profiles', array['student_profiles_admin_full_access', 'student_profiles_select_inquiring', 'student_profiles_select_own', 'student_profiles_update_own'], 'public.student_profiles has exactly its expected policies');
select has_table('public', 'supervisor_profiles', 'public.supervisor_profiles exists');
select columns_are('public', 'supervisor_profiles', array['user_id', 'display_name', 'contact_email', 'external_url', 'about_me', 'accepting_inquiries', 'updated_at'], 'public.supervisor_profiles has exactly its expected columns');
select policies_are('public', 'supervisor_profiles', array['supervisor_profiles_admin_full_access', 'supervisor_profiles_select_all', 'supervisor_profiles_update_own'], 'public.supervisor_profiles has exactly its expected policies');
select has_table('public', 'topics', 'public.topics exists');
select columns_are('public', 'topics', array['id', 'name'], 'public.topics has exactly its expected columns');
select policies_are('public', 'topics', array['topics_select_all'], 'public.topics has exactly its expected policies');
select has_table('public', 'projects', 'public.projects exists');
select columns_are('public', 'projects', array['id', 'supervisor_id', 'title', 'description', 'external_url', 'status', 'posted_at', 'updated_at'], 'public.projects has exactly its expected columns');
select policies_are('public', 'projects', array['projects_admin_full_access', 'projects_delete_own', 'projects_insert_own', 'projects_select_all', 'projects_update_own'], 'public.projects has exactly its expected policies');
select has_table('public', 'supervisor_topic_interests', 'public.supervisor_topic_interests exists');
select columns_are('public', 'supervisor_topic_interests', array['supervisor_id', 'topic_id'], 'public.supervisor_topic_interests has exactly its expected columns');
select policies_are('public', 'supervisor_topic_interests', array['supervisor_topic_interests_admin_full_access', 'supervisor_topic_interests_delete_own', 'supervisor_topic_interests_insert_own', 'supervisor_topic_interests_select_all'], 'public.supervisor_topic_interests has exactly its expected policies');
select has_table('public', 'student_topic_interests', 'public.student_topic_interests exists');
select columns_are('public', 'student_topic_interests', array['student_id', 'topic_id'], 'public.student_topic_interests has exactly its expected columns');
select policies_are('public', 'student_topic_interests', array['student_topic_interests_admin_full_access', 'student_topic_interests_delete_own', 'student_topic_interests_insert_own', 'student_topic_interests_select_own'], 'public.student_topic_interests has exactly its expected policies');
select has_table('public', 'project_topic_tags', 'public.project_topic_tags exists');
select columns_are('public', 'project_topic_tags', array['project_id', 'topic_id'], 'public.project_topic_tags has exactly its expected columns');
select policies_are('public', 'project_topic_tags', array['project_topic_tags_admin_full_access', 'project_topic_tags_delete_own', 'project_topic_tags_insert_own', 'project_topic_tags_select_all'], 'public.project_topic_tags has exactly its expected policies');
select has_table('public', 'inquiries', 'public.inquiries exists');
select columns_are('public', 'inquiries', array['id', 'student_id', 'supervisor_id', 'project_id', 'status', 'message', 'created_at', 'updated_at'], 'public.inquiries has exactly its expected columns');
select policies_are('public', 'inquiries', array['inquiries_admin_full_access', 'inquiries_insert_own', 'inquiries_select_involved', 'inquiries_update_involved'], 'public.inquiries has exactly its expected policies');
select has_table('public', 'inquiry_attachments', 'public.inquiry_attachments exists');
select columns_are('public', 'inquiry_attachments', array['id', 'inquiry_id', 'storage_path', 'mime_type', 'size_bytes', 'created_at'], 'public.inquiry_attachments has exactly its expected columns');
select policies_are('public', 'inquiry_attachments', array['inquiry_attachments_admin_full_access', 'inquiry_attachments_delete_own', 'inquiry_attachments_insert_own', 'inquiry_attachments_select_involved'], 'public.inquiry_attachments has exactly its expected policies');
select has_table('public', 'student_saved_projects', 'public.student_saved_projects exists');
select columns_are('public', 'student_saved_projects', array['student_id', 'project_id', 'saved_at'], 'public.student_saved_projects has exactly its expected columns');
select policies_are('public', 'student_saved_projects', array['student_saved_projects_admin_full_access', 'student_saved_projects_delete_own', 'student_saved_projects_insert_own', 'student_saved_projects_select_own'], 'public.student_saved_projects has exactly its expected policies');
select ok((select bool_and(relrowsecurity) from pg_class where oid = any(array['public.student_profiles'::regclass, 'public.supervisor_profiles'::regclass, 'public.topics'::regclass, 'public.projects'::regclass, 'public.supervisor_topic_interests'::regclass, 'public.student_topic_interests'::regclass, 'public.project_topic_tags'::regclass, 'public.inquiries'::regclass, 'public.inquiry_attachments'::regclass, 'public.student_saved_projects'::regclass])), 'RLS is enabled on every new table');

-- keys
select col_is_pk('public', 'student_profiles', 'user_id', 'student_profiles is keyed by user_id');
select col_is_pk('public', 'supervisor_profiles', 'user_id', 'supervisor_profiles is keyed by user_id');
select col_is_pk('public', 'topics', 'id', 'topics is keyed by id');
select col_is_pk('public', 'projects', 'id', 'projects is keyed by id');
select col_is_pk('public', 'inquiries', 'id', 'inquiries is keyed by id');
select col_is_pk('public', 'inquiry_attachments', 'id', 'inquiry_attachments is keyed by id');
select col_is_pk('public', 'supervisor_topic_interests', array['supervisor_id', 'topic_id'], 'supervisor_topic_interests is keyed by its two ids');
select col_is_pk('public', 'student_topic_interests', array['student_id', 'topic_id'], 'student_topic_interests is keyed by its two ids');
select col_is_pk('public', 'project_topic_tags', array['project_id', 'topic_id'], 'project_topic_tags is keyed by its two ids');
select col_is_pk('public', 'student_saved_projects', array['student_id', 'project_id'], 'student_saved_projects is keyed by its two ids');
select fk_ok('public', 'student_profiles', 'user_id', 'public', 'users', 'id', 'student_profiles.user_id references users.id');
select fk_ok('public', 'supervisor_profiles', 'user_id', 'public', 'users', 'id', 'supervisor_profiles.user_id references users.id');
select fk_ok('public', 'projects', 'supervisor_id', 'public', 'supervisor_profiles', 'user_id', 'projects.supervisor_id references supervisor_profiles.user_id');
select fk_ok('public', 'supervisor_topic_interests', 'supervisor_id', 'public', 'supervisor_profiles', 'user_id', 'supervisor_topic_interests.supervisor_id references supervisor_profiles.user_id');
select fk_ok('public', 'supervisor_topic_interests', 'topic_id', 'public', 'topics', 'id', 'supervisor_topic_interests.topic_id references topics.id');
select fk_ok('public', 'student_topic_interests', 'student_id', 'public', 'student_profiles', 'user_id', 'student_topic_interests.student_id references student_profiles.user_id');
select fk_ok('public', 'student_topic_interests', 'topic_id', 'public', 'topics', 'id', 'student_topic_interests.topic_id references topics.id');
select fk_ok('public', 'project_topic_tags', 'project_id', 'public', 'projects', 'id', 'project_topic_tags.project_id references projects.id');
select fk_ok('public', 'project_topic_tags', 'topic_id', 'public', 'topics', 'id', 'project_topic_tags.topic_id references topics.id');
select fk_ok('public', 'inquiries', 'student_id', 'public', 'student_profiles', 'user_id', 'inquiries.student_id references student_profiles.user_id');
select fk_ok('public', 'inquiries', 'supervisor_id', 'public', 'supervisor_profiles', 'user_id', 'inquiries.supervisor_id references supervisor_profiles.user_id');
select fk_ok('public', 'inquiries', 'project_id', 'public', 'projects', 'id', 'inquiries.project_id references projects.id');
select fk_ok('public', 'inquiry_attachments', 'inquiry_id', 'public', 'inquiries', 'id', 'inquiry_attachments.inquiry_id references inquiries.id');
select fk_ok('public', 'student_saved_projects', 'student_id', 'public', 'student_profiles', 'user_id', 'student_saved_projects.student_id references student_profiles.user_id');
select fk_ok('public', 'student_saved_projects', 'project_id', 'public', 'projects', 'id', 'student_saved_projects.project_id references projects.id');

-- types, indexes, functions and triggers
select enum_has_labels('public', 'project_status', array['available', 'unavailable', 'archived'], 'project_status has exactly its three values');
select enum_has_labels('public', 'inquiry_status', array['pending', 'viewed', 'responded', 'closed'], 'inquiry_status has exactly its four values');
select index_is_unique('public', 'topics', 'topics_name_lower_key', 'topic names are unique ignoring case');
select is_definer('public', 'get_or_create_topic', 'get_or_create_topic() is SECURITY DEFINER');
select function_privs_are('public', 'get_or_create_topic', array['text'], 'authenticated', array['EXECUTE'], 'signed-in users can call get_or_create_topic()');
select function_privs_are('public', 'get_or_create_topic', array['text'], 'anon', array[]::text[], 'anonymous visitors cannot call get_or_create_topic()');
select is_definer('public', 'handle_new_profile', 'handle_new_profile() is SECURITY DEFINER');
select trigger_is('public', 'users', 'users_create_profile', 'public', 'handle_new_profile', 'users_create_profile runs handle_new_profile()');
select trigger_is('public', 'student_profiles', 'student_profiles_set_updated_at', 'public', 'set_updated_at', 'student_profiles keeps updated_at current');
select trigger_is('public', 'supervisor_profiles', 'supervisor_profiles_set_updated_at', 'public', 'set_updated_at', 'supervisor_profiles keeps updated_at current');
select trigger_is('public', 'projects', 'projects_set_updated_at', 'public', 'set_updated_at', 'projects keeps updated_at current');
select trigger_is('public', 'inquiries', 'inquiries_set_updated_at', 'public', 'set_updated_at', 'inquiries keeps updated_at current');
select trigger_is('public', 'inquiries', 'inquiries_limit_updates', 'public', 'limit_inquiry_updates', 'inquiries_limit_updates runs limit_inquiry_updates()');

-- storage
select ok(exists (select 1 from pg_policies where schemaname = 'storage' and tablename = 'objects' and policyname = 'inquiry_attachments_files_upload'), 'storage policy inquiry_attachments_files_upload exists');
select ok(exists (select 1 from pg_policies where schemaname = 'storage' and tablename = 'objects' and policyname = 'inquiry_attachments_files_read'), 'storage policy inquiry_attachments_files_read exists');
select ok(exists (select 1 from pg_policies where schemaname = 'storage' and tablename = 'objects' and policyname = 'inquiry_attachments_files_delete'), 'storage policy inquiry_attachments_files_delete exists');
select ok(exists (select 1 from pg_policies where schemaname = 'storage' and tablename = 'objects' and policyname = 'inquiry_attachments_files_admin_full_access'), 'storage policy inquiry_attachments_files_admin_full_access exists');

select * from finish();
rollback;
