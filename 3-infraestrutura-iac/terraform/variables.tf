variable "aws_region" {
  description = "Região AWS (São Paulo para baixa latência no Brasil)"
  type        = string
  default     = "sa-east-1"
}

variable "environment" {
  description = "Ambiente de implantação"
  type        = string
  default     = "prod"
}

variable "project_name" {
  description = "Prefixo dos recursos"
  type        = string
  default     = "salao-admin"
}

variable "db_username" {
  description = "Usuário master do PostgreSQL"
  type        = string
  default     = "salao_admin"
}

variable "db_password" {
  description = "Senha do banco (injetar via TF_VAR_db_password ou AWS Secrets Manager)"
  type        = string
  sensitive   = true
}
