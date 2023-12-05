using Amazon.DynamoDBv2;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using Utils.IO.Server.Models.Entity;

namespace Utils.IO.Server.Services
{
    public interface ISqsPublisher
    {
        Task<SendMessageResponse> PublishAsync<T>(string queueName, Dictionary<string, MessageAttributeValue> messageAttributes, T message) where T : IMessage;
    }
    public class SqsPublisher : ISqsPublisher
    {

        private IAmazonSQS AmazonSQS { get; set; }

        public SqsPublisher(IAmazonSQS amazonSQS)
        {
            AmazonSQS = amazonSQS;
        }

        public async Task<SendMessageResponse> PublishAsync<T>(string queueName, Dictionary<string, MessageAttributeValue> messageAttributes, T message) where T: IMessage
        {
            var queue = await AmazonSQS.GetQueueUrlAsync(queueName);
            var messageResponse = await AmazonSQS.SendMessageAsync(new SendMessageRequest
            {
                   QueueUrl = queue.QueueUrl,
                   MessageBody = JsonSerializer.Serialize(message),
                   MessageAttributes = messageAttributes
            });

            return messageResponse;
        } 
    }
}
