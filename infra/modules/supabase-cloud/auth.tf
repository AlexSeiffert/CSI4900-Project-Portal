# Auth config (site URL, JWT expiry, signup rules) and custom access-token hook registration.
resource "supabase_settings" "this" {
  project_ref = supabase_project.this.id

  auth = jsonencode({
    site_url       = "http://localhost:3000"
    jwt_exp        = 3600
    disable_signup = false

    hook_custom_access_token_enabled = true
    hook_custom_access_token_uri     = "pg-functions://postgres/public/custom_access_token_hook"
  })
}
