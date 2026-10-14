terraform {
  required_version = "1.16.3"

  required_providers {
    azuread = {
      source  = "hashicorp/azuread"
      version = "3.10.0"
    }
  }
}

provider "azuread" {
  tenant_id = var.tenant_id
}

resource "azuread_application" "this" {
  count = var.tenant_id != null ? 1 : 0

  display_name     = var.app_display_name
  sign_in_audience = "AzureADMyOrg"

  web {
    redirect_uris = var.redirect_uris
  }

  password {
    display_name = "terraform-managed"
  }
}

resource "azuread_service_principal" "this" {
  count = var.tenant_id != null ? 1 : 0

  client_id = azuread_application.this[0].client_id
}
