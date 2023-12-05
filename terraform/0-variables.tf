variable "queue_name" {
    description = "Name of queue"
    type = string
}

variable "dynamodb_table" {
    description = "DynamoDB table name"
    type = string
}

variable "dynamodb_results_table" {
    description = "DynamoDB results table name"
    type = string
}

variable "retention_period" {
    description = "Retention period of message in queue"
    type = number
    default = 80000
}

variable "visibility_timeout" {
    description = "How long does the consumers have before the message is removed"
    type = number
    default = 60
}

variable "receive_count" {
    description = "The number of time the message can be received before going into dl queue"
    type = number
    default = 3
}