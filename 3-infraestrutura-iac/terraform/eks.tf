module "eks" {
  source  = "terraform-aws-modules/eks/aws"
  version = "~> 20.24"

  cluster_name    = "${var.project_name}-eks"
  cluster_version = "1.30"

  vpc_id                         = module.vpc.vpc_id
  subnet_ids                     = module.vpc.private_subnets
  cluster_endpoint_public_access = true

  eks_managed_node_groups = {
    microservices = {
      name           = "ms-nodes"
      instance_types = ["t3.medium"]
      min_size       = 3 # 1 nó mínimo por AZ
      max_size       = 9
      desired_size   = 3
    }
  }
}
