// Sets up the database: applies each migration whose objects aren't there yet. See README.md.
import { execFileSync } from 'node:child_process';
import { readdirSync } from 'node:fs';
import { fileURLToPath } from 'node:url';

try {
  process.loadEnvFile(new URL('../../.env', import.meta.url));
} catch {
  // no .env, use existing environmental variables
}

// One check per migration, should return true once each sql file is written to the database
const CHECKS = {
  '0001_users_and_role.sql': "select to_regclass('public.users') is not null",
  '0002_handle_new_user.sql': "select exists (select 1 from pg_trigger where tgname = 'on_auth_user_created')",
  '0003_rls_users.sql': "select exists (select 1 from pg_trigger where tgname = 'users_prevent_role_change')",
  '0004_custom_access_token_hook.sql': "select to_regprocedure('public.custom_access_token_hook(jsonb)') is not null",
};

function fail(message) {
  console.error(message);
  process.exit(1);
}

function findPsql() {
  for (const bin of [process.env.PSQL, 'psql', '/opt/homebrew/opt/libpq/bin/psql', '/usr/local/opt/libpq/bin/psql']) {
    if (!bin) continue;
    try {
      execFileSync(bin, ['--version'], { stdio: 'ignore' });
      return bin;
    } catch {
      // not this one
    }
  }
  return fail('psql not found. Install it (brew install libpq) or set PSQL to its path.');
}

function databaseUrl() {
  if (process.env.DATABASE_URL) return process.env.DATABASE_URL;
  try {
    return execFileSync('terraform', ['output', '-raw', 'db_connection_string'], {
      cwd: fileURLToPath(new URL('../../infra/modules/supabase-cloud', import.meta.url)),
      encoding: 'utf8',
    }).trim();
  } catch {
    return fail('Set DATABASE_URL, or run with `mise x --` so Terraform can provide the connection string.');
  }
}

const bin = findPsql();
const url = databaseUrl();
const psql = (...args) => execFileSync(bin, [url, '-v', 'ON_ERROR_STOP=1', '-qtAX', ...args], { encoding: 'utf8', stdio: ['ignore', 'pipe', 'pipe'] }).trim();

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
    psql('-f', dir + file);
    if (psql('-c', check) !== 't') fail(`Applied ${file}, but its check still says it is missing.`);
    console.log(`applied  ${file}`);
  }
} catch (error) {
  fail(String(error.stderr || error.message).trim());
}
