// Shared by the seed scripts: the Supabase Admin API (users.mjs) and psql (database.mjs).
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const PROJECT_NAME = 'CSI-4900 Project Portal';

try {
  process.loadEnvFile(new URL('../../.env', import.meta.url));
} catch {
  // no .env: rely on the environment
}

export function fail(message) {
  console.error(message);
  process.exit(1);
}

async function request(method, url, headers, body) {
  const res = await fetch(url, {
    method,
    headers: { 'Content-Type': 'application/json', Prefer: 'return=representation', ...headers },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  const text = await res.text();
  let json = text;
  try {
    json = text ? JSON.parse(text) : null;
  } catch {
    // non-JSON body, keep the text
  }
  if (!res.ok) fail(`${method} ${url} -> ${res.status} ${typeof json === 'string' ? json : JSON.stringify(json)}`);
  return json;
}

/** Connects to the live project's Admin API: auth() for logins, rest() for public.users. */
export async function connect() {
  const mgmtToken = process.env.SUPABASE_ACCESS_TOKEN ?? process.env.TF_VAR_supabase_access_token;
  const mgmt = { Authorization: `Bearer ${mgmtToken}` };
  let ref = process.env.SUPABASE_PROJECT_REF;
  let key = process.env.SUPABASE_SECRET_KEY;

  if ((!ref || !key) && !mgmtToken) {
    fail('Set SUPABASE_PROJECT_REF + SUPABASE_SECRET_KEY, or a Management API token (TF_VAR_supabase_access_token in .env).');
  }
  if (!ref) {
    const projects = await request('GET', 'https://api.supabase.com/v1/projects', mgmt);
    const matches = projects.filter((p) => p.name === PROJECT_NAME);
    if (matches.length !== 1) fail(`Expected one project named "${PROJECT_NAME}", found ${matches.length}. Set SUPABASE_PROJECT_REF.`);
    ref = matches[0].id;
  }
  if (!key) {
    const keys = await request('GET', `https://api.supabase.com/v1/projects/${ref}/api-keys?reveal=true`, mgmt);
    key = keys.find((k) => k.type === 'secret')?.api_key;
    if (!key) fail('No secret API key found for the project.');
  }
  const base = `https://${ref}.supabase.co`;
  const headers = { apikey: key };
  return {
    auth: (method, path, body) => request(method, `${base}/auth/v1/admin/users${path}`, headers, body),
    rest: (method, path, body) => request(method, `${base}/rest/v1/users${path}`, headers, body),
  };
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

/** Returns psql(...args), which runs psql against the live database and returns its trimmed output. */
export function connectPsql() {
  const bin = findPsql();
  const url = databaseUrl();
  return (args, input) =>
    execFileSync(bin, [url, '-v', 'ON_ERROR_STOP=1', '-qtAX', ...args], {
      encoding: 'utf8',
      input,
      maxBuffer: 64 * 1024 * 1024,
      stdio: [input === undefined ? 'ignore' : 'pipe', 'pipe', 'pipe'],
    }).trim();
}
