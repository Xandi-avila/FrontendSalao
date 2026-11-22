resource "aws_sqs_queue" "eventos_dlq" {
  name                      = "${var.project_name}-eventos-dlq"
  message_retention_seconds = 1209600 # 14 dias
}

resource "aws_sqs_queue" "eventos" {
  name                       = "${var.project_name}-eventos-queue"
  visibility_timeout_seconds = 60
  message_retention_seconds  = 345600 # 4 dias
  redrive_policy = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.eventos_dlq.arn
    maxReceiveCount     = 3
  })
}

resource "aws_iam_role" "lambda_exec" {
  name = "${var.project_name}-lambda-notificacoes-role"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action    = "sts:AssumeRole"
      Effect    = "Allow"
      Principal = { Service = "lambda.amazonaws.com" }
    }]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_sqs" {
  role       = aws_iam_role.lambda_exec.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaSQSQueueExecutionRole"
}

data "archive_file" "lambda_zip" {
  type        = "zip"
  source_dir  = "${path.module}/../../2-aplicacao/serverless-notificacoes"
  output_path = "${path.module}/build/notificacao_lambda.zip"
}

resource "aws_lambda_function" "notificacoes" {
  function_name    = "${var.project_name}-notificacao-worker"
  role             = aws_iam_role.lambda_exec.arn
  handler          = "lambda_handler.lambda_handler"
  runtime          = "python3.12"
  filename         = data.archive_file.lambda_zip.output_path
  source_code_hash = data.archive_file.lambda_zip.output_base64sha256
  timeout          = 30
  memory_size      = 256
  tracing_config {
    mode = "Active" # AWS X-Ray distribuído
  }
}

resource "aws_lambda_event_source_mapping" "sqs_trigger" {
  event_source_arn                   = aws_sqs_queue.eventos.arn
  function_name                      = aws_lambda_function.notificacoes.arn
  batch_size                         = 10
  function_response_types            = ["ReportBatchItemFailures"]
}
