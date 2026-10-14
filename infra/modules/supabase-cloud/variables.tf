variable "supabase_access_token" {
  description = "Supabase Management API personal access token (needs Organizations: Read, Organization Projects: Read-write). Set via TF_VAR_supabase_access_token in .env — never set a default here."
  type        = string
  sensitive   = true
}

variable "organization_id" {
  description = "Supabase organization slug"
  type        = string
  default     = "aedfbzypupdokvuvreld" # uoprojects
}

variable "project_name" {
  description = "Name of the Supabase project"
  type        = string
  default     = "CSI-4900 Project Portal"
}

variable "region" {
  description = "Region the project's compute and database run in"
  type        = string
  default     = "us-east-1"
}

variable "db_password" {
  description = "Postgres superuser password for the project. Set via TF_VAR_db_password in .env — never set a default here."
  type        = string
  sensitive   = true
}

variable "azure_tenant_id" {
  description = "uOttawa Azure AD tenant ID, for Azure SSO login. Not yet available — blocked on uOttawa IT (see infra/azuread). Leave null until then; Azure SSO stays disabled without it."
  type        = string
  default     = null
}

variable "azure_client_id" {
  description = "Azure AD application (client) ID for Azure SSO — from infra/azuread's client_id output, once tenant access exists."
  type        = string
  default     = null
}

variable "azure_client_secret" {
  description = "Azure AD application client secret for Azure SSO — from infra/azuread's client_secret output, once tenant access exists."
  type        = string
  default     = null
  sensitive   = true
}
