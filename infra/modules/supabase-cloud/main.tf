terraform {
  required_version = "1.16.3" # matches terraform pin in .tool-versions

  required_providers {
    supabase = {
      source  = "supabase/supabase"
      version = "1.11.0"
    }
  }
}

provider "supabase" {
  access_token = var.supabase_access_token
}

resource "supabase_project" "this" {
  organization_id         = var.organization_id
  name                    = var.project_name
  database_password       = var.db_password
  region                  = var.region
  legacy_api_keys_enabled = false
}

data "supabase_pooler" "this" {
  project_ref = supabase_project.this.id
}
