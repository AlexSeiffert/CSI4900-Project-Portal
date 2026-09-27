output "project_ref" {
  description = "Supabase project reference — used in API URLs, dashboard links, and by later migrations/tickets"
  value       = supabase_project.this.id
}

output "api_url" {
  description = "Base URL for the project's Data API, Auth, and Storage endpoints"
  value       = "https://${supabase_project.this.id}.supabase.co"
}

output "db_connection_string" {
  description = "Pooled Postgres connection string (transaction mode)"
  value       = replace(data.supabase_pooler.this.url["transaction"], "[YOUR-PASSWORD]", var.db_password)
  sensitive   = true
}
