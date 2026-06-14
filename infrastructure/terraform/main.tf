terraform {
  required_version = ">= 1.6"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "main" {
  name     = "rg-workpac-recruitment-${var.environment}"
  location = var.location
  tags     = var.tags
}

resource "azurerm_container_app_environment" "env" {
  name                       = "cae-workpac-${var.environment}"
  location                   = azurerm_resource_group.main.location
  resource_group_name        = azurerm_resource_group.main.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
}

resource "azurerm_log_analytics_workspace" "main" {
  name                = "log-workpac-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
}

resource "azurerm_container_app" "applications_api" {
  name                         = "ca-applications-${var.environment}"
  container_app_environment_id = azurerm_container_app_environment.env.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Multiple"

  template {
    container {
      name   = "applications-api"
      image  = "${var.container_registry}/workpac/applications-api:${var.image_tag}"
      cpu    = var.api_cpu
      memory = var.api_memory

      env {
        name  = "InfrastructureMode"
        value = "Azure"
      }
      env {
        name  = "ConnectionStrings__Database"
        value = azurerm_mssql_database.main.connection_string
      }
    }
    min_replicas = var.environment == "prod" ? 2 : 1
    max_replicas = var.environment == "prod" ? 10 : 3
  }

  ingress {
    external   = true
    target_port = 8080
    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  tags = var.tags
}

resource "azurerm_container_app" "matching_service" {
  name                         = "ca-matching-${var.environment}"
  container_app_environment_id = azurerm_container_app_environment.env.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Multiple"

  template {
    container {
      name   = "matching-service"
      image  = "${var.container_registry}/workpac/matching-service:${var.image_tag}"
      cpu    = var.service_cpu
      memory = var.service_memory

      env {
        name  = "InfrastructureMode"
        value = "Azure"
      }
      env {
        name  = "ConnectionStrings__Database"
        value = azurerm_mssql_database.main.connection_string
      }
      env {
        name  = "RabbitMq__Host"
        value = azurerm_servicebus_namespace.main.name
      }
    }
    min_replicas = 1
    max_replicas = 5
  }

  tags = var.tags
}

resource "azurerm_mssql_server" "main" {
  name                         = "sql-workpac-${var.environment}"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_login
  administrator_login_password = var.sql_admin_password
}

resource "azurerm_mssql_database" "main" {
  name         = "sqldb-workpac-${var.environment}"
  server_id    = azurerm_mssql_server.main.id
  sku_name     = var.environment == "prod" ? "S2" : "S0"
  max_size_gb  = var.environment == "prod" ? 50 : 10
}

resource "azurerm_servicebus_namespace" "main" {
  name                = "sb-workpac-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "Standard"
}

resource "azurerm_servicebus_topic" "events" {
  name         = "workpac-events"
  namespace_id = azurerm_servicebus_namespace.main.id
}

resource "azurerm_servicebus_subscription" "matching" {
  name               = "matching-service"
  topic_id           = azurerm_servicebus_topic.events.id
  max_delivery_count = 3
}

resource "azurerm_application_insights" "main" {
  name                = "ai-workpac-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  application_type    = "web"
}

resource "azurerm_storage_account" "docs" {
  name                     = "stworkpacdocs${var.environment}"
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_container" "applications" {
  name                 = "applications"
  storage_account_name = azurerm_storage_account.docs.name
}

resource "azurerm_redis_cache" "matching" {
  name                = "redis-workpac-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  capacity            = var.environment == "prod" ? 2 : 1
  family              = "C"
  sku_name            = "Standard"
}
