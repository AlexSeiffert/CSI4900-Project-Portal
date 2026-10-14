output "client_id" {
  description = "Application (client) ID — null until tenant_id is set and the app registration exists"
  value       = try(azuread_application.this[0].client_id, null)
}

output "service_principal_object_id" {
  description = "Service principal object ID — null until tenant_id is set"
  value       = try(azuread_service_principal.this[0].object_id, null)
}

output "client_secret" {
  description = "Azure AD application client secret — null until tenant_id is set. Sensitive; feed into infra/modules/supabase-cloud's azure_client_secret variable to wire up Azure SSO."
  value       = try(tolist(azuread_application.this[0].password)[0].value, null)
  sensitive   = true
}
