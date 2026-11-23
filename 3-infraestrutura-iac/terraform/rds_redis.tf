resource "aws_db_subnet_group" "salao" {
  name       = "${var.project_name}-db-subnet-group"
  subnet_ids = module.vpc.database_subnets
}

resource "aws_db_instance" "postgres" {
  identifier             = "${var.project_name}-postgres"
  engine                 = "postgres"
  engine_version         = "16.3"
  instance_class         = "db.t4g.medium"
  allocated_storage      = 50
  max_allocated_storage  = 200
  db_name                = "salao_db"
  username               = var.db_username
  password               = var.db_password
  db_subnet_group_name   = aws_db_subnet_group.salao.name
  multi_az               = true # Failover automático entre AZs
  storage_encrypted      = true
  backup_retention_period = 7
  skip_final_snapshot       = false
  final_snapshot_identifier = "${var.project_name}-postgres-final"
  vpc_security_group_ids    = [aws_security_group.dados.id]
}

# Banco e cache só aceitam conexões vindas de dentro da VPC
resource "aws_security_group" "dados" {
  name   = "${var.project_name}-dados"
  vpc_id = module.vpc.vpc_id

  ingress {
    description = "PostgreSQL a partir da VPC"
    from_port   = 5432
    to_port     = 5432
    protocol    = "tcp"
    cidr_blocks = [module.vpc.vpc_cidr_block]
  }

  ingress {
    description = "Redis a partir da VPC"
    from_port   = 6379
    to_port     = 6379
    protocol    = "tcp"
    cidr_blocks = [module.vpc.vpc_cidr_block]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_elasticache_subnet_group" "redis" {
  name       = "${var.project_name}-redis-subnets"
  subnet_ids = module.vpc.private_subnets
}

resource "aws_elasticache_replication_group" "redis" {
  replication_group_id       = "${var.project_name}-redis"
  description                = "Cache distribuído Multi-AZ do Salão Admin"
  engine                     = "redis"
  node_type                  = "cache.t4g.micro"
  num_cache_clusters         = 2
  automatic_failover_enabled = true
  multi_az_enabled           = true
  subnet_group_name          = aws_elasticache_subnet_group.redis.name
  security_group_ids         = [aws_security_group.dados.id]
}
