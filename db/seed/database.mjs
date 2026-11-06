// Sets up the database: applies each migration whose objects aren't there yet. See README.md.
import { readdirSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { connectPsql, fail } from './lib.mjs';

// One check per migration, should return true once each sql file is written to the database
const CHECKS = {
  '0001_users_and_role.sql': "select to_regclass('public.users') is not null",
  '0002_handle_new_user.sql': "select exists (select 1 from pg_trigger where tgname = 'on_auth_user_created')",
  '0003_rls_users.sql': "select exists (select 1 from pg_trigger where tgname = 'users_prevent_role_change')",
  '0004_custom_access_token_hook.sql': "select to_regprocedure('public.custom_access_token_hook(jsonb)') is not null",
  '0005_profiles.sql': "select exists (select 1 from pg_trigger where tgname = 'users_create_profile')",
  '0006_topics.sql': "select to_regprocedure('public.get_or_create_topic(text)') is not null",
  '0007_projects.sql': "select to_regclass('public.projects') is not null",
  '0008_project_topic_tags.sql': "select to_regclass('public.project_topic_tags') is not null",
  '0009_inquiries.sql': "select to_regclass('public.inquiries') is not null",
  '0010_inquiry_attachments.sql': "select to_regclass('public.inquiry_attachments') is not null",
  '0011_saved_projects.sql': "select to_regclass('public.student_saved_projects') is not null",
  '0012_topic_tags.sql': "select exists (select 1 from public.topics where name = 'Games & Graphics')",
};

const run = connectPsql();
const psql = (...args) => run(args);

const dir = fileURLToPath(new URL('../migrations/', import.meta.url));
const files = readdirSync(dir).filter((f) => f.endsWith('.sql')).sort();

try {
  for (const file of files) {
    const check = CHECKS[file];
    if (!check) fail(`No check for ${file}: add one to CHECKS in db/seed/database.mjs.`);
    if (psql('-c', check) === 't') {
      console.log(`present  ${file}`);
      continue;
    }
    psql('-1', '-f', dir + file); // one transaction: a failed migration leaves nothing behind
    if (psql('-c', check) !== 't') fail(`Applied ${file}, but its check still says it is missing.`);
    console.log(`applied  ${file}`);
  }
} catch (error) {
  fail(String(error.stderr || error.message).trim());
}
