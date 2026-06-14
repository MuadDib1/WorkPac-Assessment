output "applications_api_url" {
  value = azurerm_container_app.applications_api.latest_revision_fqdn
}

output "application_insights_key" {
  value     = azurerm_application_insights.main.instrumentation_key
  sensitive = true
}

output "storage_connection_string" {
  value     = azurerm_storage_account.docs.primary_connection_string
  sensitive = true
}

output "servicebus_connection_string" {
  value     = azurerm_servicebus_namespace.main.default_primary_connection_string
  sensitive = true
}

output "redis_connection_string" {
  value     = azurerm_redis_cache.matching.primary_connection_string
  sensitive = true
}
