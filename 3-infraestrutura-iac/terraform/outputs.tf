output "eks_cluster_name" { value = module.eks.cluster_name }
output "rds_endpoint"     { value = aws_db_instance.postgres.endpoint }
output "sqs_queue_url"    { value = aws_sqs_queue.eventos.url }
output "lambda_arn"       { value = aws_lambda_function.notificacoes.arn }
