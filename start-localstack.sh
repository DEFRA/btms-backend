#!/bin/bash
export AWS_REGION=eu-west-2
export AWS_DEFAULT_REGION=eu-west-2
export AWS_ACCESS_KEY_ID=local
export AWS_SECRET_ACCESS_KEY=local

awslocal sns create-topic --attributes FifoTopic=true --name customs_clearance_request.fifo
awslocal sns create-topic --attributes FifoTopic=true --name customs_finalisation_notification.fifo
awslocal sns create-topic --attributes FifoTopic=true --name customs_error_notification.fifo
awslocal sns create-topic --attributes FifoTopic=true --name alvs_decision_notification.fifo
awslocal sns create-topic --attributes FifoTopic=true --name alvs_error_notification.fifo

awslocal sqs create-queue --attributes FifoQueue=true --queue-name customs_clearance_request.fifo
awslocal sqs create-queue --attributes FifoQueue=true --queue-name customs_finalisation_notification.fifo
awslocal sqs create-queue --attributes FifoQueue=true --queue-name customs_error_notification.fifo
awslocal sqs create-queue --attributes FifoQueue=true --queue-name alvs_decision_notification.fifo
awslocal sqs create-queue --attributes FifoQueue=true --queue-name alvs_error_notification.fifo

SNS_ARN=arn:aws:sns:eu-west-2:000000000000
SQS_ARN=arn:aws:sqs:eu-west-2:000000000000

awslocal sns subscribe --topic-arn $SNS_ARN:customs_clearance_request.fifo          --protocol sqs --notification-endpoint $SQS_ARN:customs_clearance_request.fifo
awslocal sns subscribe --topic-arn $SNS_ARN:customs_finalisation_notification.fifo  --protocol sqs --notification-endpoint $SQS_ARN:customs_finalisation_notification.fifo
awslocal sns subscribe --topic-arn $SNS_ARN:customs_error_notification.fifo         --protocol sqs --notification-endpoint $SQS_ARN:customs_error_notification.fifo
awslocal sns subscribe --topic-arn $SNS_ARN:alvs_decision_notification.fifo         --protocol sqs --notification-endpoint $SQS_ARN:alvs_decision_notification.fifo
awslocal sns subscribe --topic-arn $SNS_ARN:alvs_error_notification.fifo            --protocol sqs --notification-endpoint $SQS_ARN:alvs_error_notification.fifo
