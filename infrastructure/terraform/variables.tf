variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "Australia East"
}

variable "tags" {
  description = "Resource tags"
  type        = map(string)
  default = {
    Project     = "WorkPac Recruitment"
    ManagedBy   = "Terraform"
    Repo        = "github.com/workpac/recruitment-platform"
  }
}

variable "container_registry" {
  description = "Container registry URL"
  type        = string
  default     = "ghcr.io"
}

variable "image_tag" {
  description = "Container image tag"
  type        = string
  default     = "latest"
}

variable "sql_admin_login" {
  description = "SQL Server admin login"
  type        = string
  sensitive   = true
}

variable "sql_admin_password" {
  description = "SQL Server admin password"
  type        = string
  sensitive   = true
}

variable "api_cpu" {
  description = "CPU for API container app"
  type        = string
  default     = "0.5"
}

variable "api_memory" {
  description = "Memory for API container app"
  type        = string
  default     = "1Gi"
}

variable "service_cpu" {
  description = "CPU for background service container app"
  type        = string
  default     = "0.25"
}

variable "service_memory" {
  description = "Memory for background service container app"
  type        = string
  default     = "0.5Gi"
}
