using System.Diagnostics;
using Amazon.SQS;
using Amazon.SQS.Model;
using Utils.IO.Server.Models.Entity;
using Newtonsoft.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using Utils.IO.Server.Utils;
using Utils.IO.Server.Services;
using Microsoft.Extensions.Options;
namespace Utils.IO.Server.Queues
{
	public class GptProcessingQueue : BackgroundService
	{
        private IAmazonSQS AmazonSQS { get; }
        private IDynamoDBService DynamoDbService { get; }

        private readonly OpenAIService OpenAIService;
        private readonly AWSConfiguration AwsConfiguration;
        public GptProcessingQueue(
            IAmazonSQS amazonSQS, 
            IDynamoDBService dynamoDBService, 
            OpenAIService openAIService, 
            AWSConfiguration awsConfiguration)
        {
            AmazonSQS = amazonSQS;
            DynamoDbService = dynamoDBService;
            OpenAIService = openAIService;
            AwsConfiguration = awsConfiguration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queue = await AmazonSQS.GetQueueUrlAsync(AwsConfiguration.GptWorkItemsTableName);
            var dlq = await AmazonSQS.GetQueueUrlAsync($"{AwsConfiguration.GptWorkItemsTableName}-dlq");

            var receiveRequest = new ReceiveMessageRequest
            {
                QueueUrl = queue.QueueUrl,
                MaxNumberOfMessages = 10,
                MessageAttributeNames = new List<string> { "All" },
                AttributeNames = new List<string> { "All" },
                VisibilityTimeout = 60
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                var receivedResponse = await AmazonSQS.ReceiveMessageAsync(receiveRequest, stoppingToken);
                var pendingStatus = ((int)GptStatuses.Pending).ToString();
                if (receivedResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine(receivedResponse.HttpStatusCode);
                    return;
                }

                foreach (var msg in receivedResponse.Messages)
                {
                    if (msg.MessageAttributes.TryGetValue("status", out MessageAttributeValue statusAttribute) && statusAttribute != null &&
                        statusAttribute.DataType == "Number" && statusAttribute.StringValue == pendingStatus)
                    {
                        var sqsMessage = JsonConvert.DeserializeObject<GptMessage>(msg.Body);
                        try
                        {

                            var (resultOfOpenAiCall, isSuccessful) = await CallOpenAIApi(sqsMessage!);
                            if(isSuccessful)
                            {
                                await DynamoDbService.UpdateWorkItemsAndCreateResult(msg.MessageId, resultOfOpenAiCall);
                            }
                            else if(!isSuccessful && sqsMessage != null)
                            {
                                await AmazonSQS.SendMessageAsync(new SendMessageRequest
                                {
                                    QueueUrl = dlq.QueueUrl,
                                    MessageBody = JsonConvert.SerializeObject(sqsMessage!),
                                    MessageAttributes = SqsUtils.GetSqsMessageAttributes(sqsMessage!)
                                });
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        finally
                        {
                            await AmazonSQS.DeleteMessageAsync(queue.QueueUrl, msg.ReceiptHandle);

                        }
                        continue;
                    }
                }
            }


        }

        private async Task<(string, bool)> CallOpenAIApi(GptMessage sqsMessage)
        {
            var result = "";
            var isSuccessful = false;
            if (sqsMessage.MessageType == DynamoDBTypes.TextToImage)
            {
                var concept = sqsMessage.Prompt.Split(":")[0];
                var size = sqsMessage.Prompt.Split(":")[1];
                var textToImageRequest = GptUtils.CreateTextToImageRequest(new TextToImageArgs(concept, size));
                var textToImageResult = await OpenAIService.Image.CreateImage(textToImageRequest);
                result = textToImageResult.Results.First().Url;
                isSuccessful = textToImageResult.Successful;
            }
            else
            {
                var chatCompletionRequest = GptUtils.CreateChatCompletionRequestBasedOnType(sqsMessage);
                var completionResult = await OpenAIService.ChatCompletion.CreateCompletion(chatCompletionRequest);
                result = completionResult.Choices.First().Message.Content;
                isSuccessful = completionResult.Successful;
            }

            return (result, isSuccessful);
        }

        
    }
}
