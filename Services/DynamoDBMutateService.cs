using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using OpenAI.ObjectModels.ResponseModels;
using System;
using Utils.IO.Server.Models.Entity;
using Utils.IO.Server.Utils;

namespace Utils.IO.Server.Services
{
    public interface IDynamoDBService
    {
        Task<T> LoadRecordAsync<T>(string id);
        Task InsertWorkItemRecordIntoDynamoDB(GptMessage message, string messageId);
        Task UpdateWorkItemsAndCreateResult(string messageId, string gptResult);
    }
    public class DynamoDBMutateService: IDynamoDBService
    {
        private IAmazonDynamoDB DynamoDBClient { get; }
        private IS3Service S3Service { get; }
        private readonly AWSConfiguration AwsConfiguration;

        public DynamoDBMutateService(
            IAmazonDynamoDB dynamoDBClient, 
            IS3Service s3Service, 
            AWSConfiguration awsConfiguration)
        {
            DynamoDBClient = dynamoDBClient;
            S3Service = s3Service;
            AwsConfiguration = awsConfiguration;
        }
        
        public async Task<T> LoadRecordAsync<T>(string id)
        {
            using var dynamoDbContext = new DynamoDBContext(DynamoDBClient);

            var item = (await dynamoDbContext.QueryAsync<T>(id).GetRemainingAsync()).First();

            return item;
        }

        public async Task InsertWorkItemRecordIntoDynamoDB(GptMessage message, string messageId)
        {
            using var dynamoDBContext = new DynamoDBContext(DynamoDBClient);

            await dynamoDBContext.SaveAsync(new GptWorkItem()
            {
                MessageId = messageId,
                MessageType = message.MessageType,
                Status = message.Status.ToString()
            });
        }
        public async Task UpdateWorkItemsAndCreateResult(string messageId, string gptResult)
        {
            using var dynamoDbContext = new DynamoDBContext(DynamoDBClient);

            var workItemToUpdate = await LoadRecordAsync<GptWorkItem>(messageId);
            var fileKey = $"{workItemToUpdate.MessageType}/{messageId}.txt";
            workItemToUpdate.Status = ((int)GptStatuses.Successful).ToString();
            await dynamoDbContext.SaveAsync(workItemToUpdate);
            await S3Service.WriteFileAndSubmitToS3(gptResult, fileKey);
            var resultUrl = S3Service.GetUrlFromS3(fileKey);
            var workResult = new GptWorkResults()
            {
                ResultId = messageId,
                ResultUrl = resultUrl,
                WorkItemId = workItemToUpdate.MessageId
            };
            await dynamoDbContext.SaveAsync(workResult);
        }

    }
}
