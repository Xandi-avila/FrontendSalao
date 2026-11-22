module "vpc" {
  source  = "terraform-aws-modules/vpc/aws"
  version = "~> 5.13"

  name = "${var.project_name}-vpc"
  cidr = "10.20.0.0/16"

  azs             = ["${var.aws_region}a", "${var.aws_region}b", "${var.aws_region}c"]
  public_subnets  = ["10.20.1.0/24", "10.20.2.0/24", "10.20.3.0/24"]
  private_subnets = ["10.20.11.0/24", "10.20.12.0/24", "10.20.13.0/24"]
  database_subnets = ["10.20.21.0/24", "10.20.22.0/24", "10.20.23.0/24"]

  enable_nat_gateway     = true
  single_nat_gateway     = false # Um NAT Gateway por AZ para Alta Disponibilidade real
  enable_dns_hostnames   = true
  enable_dns_support     = true

  public_subnet_tags = {
    "kubernetes.io/role/elb" = 1
  }
  private_subnet_tags = {
    "kubernetes.io/role/internal-elb" = 1
  }
}
