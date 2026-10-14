variable "tenant_id" {
  description = "uOttawa Azure AD (Entra ID) tenant ID. Not yet available — blocked on uOttawa IT granting tenant access. Leave null until then; every resource in this module won't work without it."
  type        = string
  default     = null
}

variable "app_display_name" {
  description = "Display name for the Azure AD app registration"
  type        = string
  default     = "CSI-4900 Project Portal"
}

variable "redirect_uris" {
  description = "OAuth redirect URIs Azure AD sends auth codes/tokens back to"
  type        = list(string)
  default     = ["http://localhost:3000/auth/callback"]
}
