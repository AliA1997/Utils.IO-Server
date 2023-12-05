provider "aws" {
  region  = "us-east-2"
  profile = "default"
}

resource "aws_dynamodb_table" "gpt-work-items" {
  name           = var.dynamodb_table
  billing_mode   = "PROVISIONED"
  read_capacity  = 1
  write_capacity = 1

  hash_key = "messageId"
  range_key = "messageType"

  attribute {
    name = "messageId"
    type = "S"
  }

  attribute {
    name = "messageType"
    type = "S"
  }

  attribute {
    name = "status"
    type = "S"
  }

  # Secondary index
  global_secondary_index {
    name               = "statusIndex"
    hash_key           = "messageId"
    range_key          = "status"
    write_capacity     = 1
    read_capacity      = 1
    projection_type    = "ALL"  # You can adjust this based on your needs
  }
}

resource "aws_dynamodb_table" "gpt-work-results" {
  name           = var.dynamodb_results_table
  billing_mode   = "PROVISIONED"
  read_capacity  = 1
  write_capacity = 1

  hash_key = "workItemId"
  range_key = "resultUrl"

  attribute {
    name = "workItemId"
    type = "S"
  }

  attribute {
    name = "resultUrl"
    type = "S"
  }

    
  attribute {
    name = "resultId"
    type = "S"
  }

  global_secondary_index {
    name               = "resultIdIndex"
    hash_key           = "workItemId"
    range_key          = "resultId"
    write_capacity     = 1
    read_capacity      = 1
    projection_type    = "ALL"
  }
}


resource "aws_sqs_queue" "sqs_notification_queue" {
  name                       = var.queue_name
  message_retention_seconds  = var.retention_period
  visibility_timeout_seconds = var.visibility_timeout
  redrive_policy = jsonencode({
    "deadLetterTargetArn" = aws_sqs_queue.sqs_notification_dlq.arn
    "maxReceiveCount"     = var.receive_count
  })
}

resource "aws_sqs_queue" "sqs_notification_dlq" {
  name                       = "${var.queue_name}-dlq"
  message_retention_seconds  = var.retention_period
  visibility_timeout_seconds = var.visibility_timeout
}