# infra/azuread

Scaffolding for future Azure AD (Entra ID) SSO, so uOttawa students can log in with their school email. This is not functional yet, blocked until uOttawa IT grants tenant access. Resources here only activate once `tenant_id` is supplied, without it, terraform apply fails immediately at provider auth, before attempting anything.
