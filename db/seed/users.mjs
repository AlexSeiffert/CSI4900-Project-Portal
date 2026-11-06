// Create, modify and remove users on the live Supabase project. Usage and config: see README.md.
import { parseArgs } from 'node:util';
import { randomBytes } from 'node:crypto';
import { connect, fail } from './lib.mjs';

const ROLES = ['student', 'supervisor', 'admin'];
const UUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

const USAGE = `Usage: node db/seed/users.mjs <command> [options]

  create  --email <email> --role <${ROLES.join('|')}> [--password <pw>]
  modify  (--email <email> | --id <uuid>) [--role <role>] [--password <pw>] [--new-email <email>]
  remove  (--email <email> | --id <uuid>) --yes`;

async function findUser(c, values) {
  if (values.id) {
    if (!UUID.test(values.id)) fail(`--id is not a uuid: ${values.id}`);
    const u = await c.auth('GET', `/${values.id}`);
    return { id: u.id, email: u.email };
  }
  const email = values.email.toLowerCase();
  const rows = await c.rest('GET', `?email=eq.${encodeURIComponent(email)}&select=id,email`);
  if (rows.length) return rows[0];
  for (let page = 1; ; page++) {
    const { users } = await c.auth('GET', `?page=${page}&per_page=200`);
    const hit = users.find((u) => u.email?.toLowerCase() === email);
    if (hit) return { id: hit.id, email: hit.email };
    if (users.length < 200) fail(`No user with email ${email}`);
  }
}

async function roleOf(c, id) {
  const rows = await c.rest('GET', `?id=eq.${id}&select=role`);
  return rows.length ? rows[0].role : '(no public.users row)';
}

function checkRole(role) {
  if (!role || !ROLES.includes(role)) fail(`--role must be one of: ${ROLES.join(', ')}`);
}

const { positionals, values } = parseArgs({
  allowPositionals: true,
  options: {
    email: { type: 'string' },
    id: { type: 'string' },
    role: { type: 'string' },
    password: { type: 'string' },
    'new-email': { type: 'string' },
    yes: { type: 'boolean' },
  },
});
const command = positionals[0];


// Commands (create, modify, remove)
if (command === 'create') {
  if (!values.email) fail(USAGE);
  checkRole(values.role);
  const c = await connect();
  const password = values.password ?? randomBytes(18).toString('base64url');
  const user = await c.auth('POST', '', {
    email: values.email.toLowerCase(),
    password,
    email_confirm: true,
    user_metadata: { role: values.role },
  });
  console.log(`created  ${user.email}  role=${await roleOf(c, user.id)}  id=${user.id}`);
  if (!values.password) console.log(`password ${password}   (shown once)`);
} else if (command === 'modify') {
  if (!values.email && !values.id) fail(USAGE);
  if (!values.role && !values.password && !values['new-email']) fail('Nothing to modify: pass --role, --password or --new-email.');
  if (values.role) checkRole(values.role);
  const c = await connect();
  const user = await findUser(c, values);
  if (values.password || values['new-email']) {
    await c.auth('PUT', `/${user.id}`, {
      ...(values.password && { password: values.password }),
      ...(values['new-email'] && { email: values['new-email'].toLowerCase(), email_confirm: true }),
    });
  }
  const patch = {
    ...(values.role && { role: values.role }),
    ...(values['new-email'] && { email: values['new-email'].toLowerCase() }),
  };
  if (Object.keys(patch).length) {
    const rows = await c.rest('PATCH', `?id=eq.${user.id}`, patch);
    if (!rows.length) fail(`${user.email} has no public.users row, so role/email there were not updated.`);
  }
  console.log(`modified ${values['new-email'] ?? user.email}  role=${await roleOf(c, user.id)}  id=${user.id}`);
} else if (command === 'remove') {
  if (!values.email && !values.id) fail(USAGE);
  const c = await connect();
  const user = await findUser(c, values);
  const role = await roleOf(c, user.id);
  if (!values.yes) fail(`dry run: would remove ${user.email} (role=${role}, id=${user.id}). Re-run with --yes to delete.`);
  await c.auth('DELETE', `/${user.id}`);
  console.log(`removed  ${user.email}  (public.users row now: ${await roleOf(c, user.id)})`);
} else {
  fail(USAGE);
}
