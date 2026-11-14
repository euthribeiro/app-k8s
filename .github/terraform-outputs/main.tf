terraform {
  required_version = ">= 1.5.0"
}

variable "tfc_organization" {
  description = "Organização no HCP Terraform."
  type        = string
}

variable "infra_workspace" {
  description = "Workspace do repositório infra-k8s (cluster EKS, ACM, ECR)."
  type        = string
}

variable "rds_workspace" {
  description = "Workspace do stack rds/ do repositório infra-db."
  type        = string
}

variable "email_workspace" {
  description = "Workspace do stack email/ do repositório infra-k8s (role IRSA do SES)."
  type        = string
}

variable "ecr_workspace" {
  description = "Workspace do stack ecr/ do repositório infra-k8s."
  type        = string
}

variable "mensageria_workspace" {
  description = "Workspace do stack mensageria/ do repositório infra-k8s (SNS, SQS e role IRSA da Saga)."
  type        = string
}

data "terraform_remote_state" "infra" {
  backend = "remote"

  config = {
    organization = var.tfc_organization
    workspaces = {
      name = var.infra_workspace
    }
  }
}

data "terraform_remote_state" "rds" {
  backend = "remote"

  config = {
    organization = var.tfc_organization
    workspaces = {
      name = var.rds_workspace
    }
  }
}

data "terraform_remote_state" "email" {
  backend = "remote"

  config = {
    organization = var.tfc_organization
    workspaces = {
      name = var.email_workspace
    }
  }
}

data "terraform_remote_state" "ecr" {
  backend = "remote"

  config = {
    organization = var.tfc_organization
    workspaces = {
      name = var.ecr_workspace
    }
  }
}

data "terraform_remote_state" "mensageria" {
  backend = "remote"

  config = {
    organization = var.tfc_organization
    workspaces = {
      name = var.mensageria_workspace
    }
  }
}

output "cluster_name" {
  value = data.terraform_remote_state.infra.outputs.cluster_name
}

output "database_hostname" {
  value = data.terraform_remote_state.rds.outputs.database_hostname
}

output "database_name" {
  value = data.terraform_remote_state.rds.outputs.database_name
}

output "app_ses_irsa_role_arn" {
  value = data.terraform_remote_state.email.outputs.app_ses_irsa_role_arn
}

output "api_repository" {
  value = data.terraform_remote_state.ecr.outputs.api_repository_url
}

output "chart_repository" {
  value = data.terraform_remote_state.ecr.outputs.chart_repository
}

output "chart_repository_url" {
  value = data.terraform_remote_state.ecr.outputs.chart_repository_url
}

# A ServiceAccount da API aponta para uma unica role IRSA: a do stack mensageria/, que
# acumula SNS, SQS e o ses:SendEmail que antes vinha da role do stack email/.
output "saga_role_arn" {
  value = data.terraform_remote_state.mensageria.outputs.os_role_arn
}

output "saga_topic_arn" {
  value = data.terraform_remote_state.mensageria.outputs.os_topic_arn
}

output "saga_queue_url" {
  value = data.terraform_remote_state.mensageria.outputs.os_queue_url
}
