using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web.Resource;
using Newtonsoft.Json;
using OpenAI.Managers;
using Stripe;
using Utils.IO.Server.Models;
using Utils.IO.Server.Models.Entity;
using Utils.IO.Server.Models.Requests;
using Utils.IO.Server.Models.Responses;
using Utils.IO.Server.Services;
using Utils.IO.Server.Utils;
using static Postgrest.Constants;

namespace Utils.IO.Server.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    //[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    public class GptController : Controller
    {

        private readonly ILogger<GptController> Logger;
        private readonly IAmazonSQS AmazonSQS;
        private readonly IDynamoDBService DynamoDbService;
        private readonly ISqsPublisher SqsPublisher;
        private readonly AWSConfiguration AwsConfiguration;
        public GptController(
            ILogger<GptController> logger, 
            IDynamoDBService dynamoDBService, 
            AWSConfiguration awsConfiguration)
        {
            Logger = logger;
            AmazonSQS = new AmazonSQSClient(Amazon.RegionEndpoint.USEast2);
            DynamoDbService = dynamoDBService;
            SqsPublisher = new SqsPublisher(AmazonSQS);
            AwsConfiguration = awsConfiguration;
        }

        [HttpGet]
        public IActionResult Test()
        {
            return Ok("Test gpt endpoint hit!");
        }

        [HttpPost("article-summarizer")]
        public async Task<WorkResponse> ArticleSummarizer([FromBody] CommonGptRequest request)
        {
            return await GptUtils.ExecuteWithExceptionHandling<WorkResponse>(async () =>
            {
                if (string.IsNullOrEmpty(request.InputText))
                    throw new Exception("Bad request");

                var prompt = PromptUtils.GenerateArticleSummarizerPrompt(request.InputText);

                var sqsMessage = InstantiateSqsMessageToSerialize(DynamoDBTypes.ArticleSummarizer, prompt);
                var sqsMessageAttributes = SqsUtils.GetSqsMessageAttributes(sqsMessage);
                var sqsMessageGuid = (await SqsPublisher.PublishAsync<GptMessage>(AwsConfiguration.GptWorkItemsTableName ?? "", sqsMessageAttributes, sqsMessage)).MessageId;

                await DynamoDbService.InsertWorkItemRecordIntoDynamoDB(sqsMessage, sqsMessageGuid);
                return new WorkResponse() { WorkGuid = sqsMessageGuid };
            });
        }
        
        [HttpPost("paragraph-generator")]
        public async Task<WorkResponse> ParagraphGenerator([FromBody] CommonGptRequest request)
        {
            return await GptUtils.ExecuteWithExceptionHandling<WorkResponse>(async () =>
            {
                if (string.IsNullOrEmpty(request.InputText))
                    throw new Exception("Bad request");

                var prompt = PromptUtils.GenerateParagraphGeneratorPrompt(request.InputText);

                var sqsMessage = InstantiateSqsMessageToSerialize(DynamoDBTypes.ParagraphGenerator, prompt);
                var sqsMessageAttributes = SqsUtils.GetSqsMessageAttributes(sqsMessage);
                var sqsMessageGuid = (await SqsPublisher.PublishAsync<GptMessage>(AwsConfiguration.GptWorkItemsTableName ?? "", sqsMessageAttributes, sqsMessage)).MessageId;

                await DynamoDbService.InsertWorkItemRecordIntoDynamoDB(sqsMessage, sqsMessageGuid);
                return new WorkResponse() { WorkGuid = sqsMessageGuid };
            });
        }

        [HttpPost("convert-code")]
        public async Task<WorkResponse> ConvertCode([FromBody] ConvertCodeRequest request)
        {
            return await GptUtils.ExecuteWithExceptionHandling<WorkResponse>(async () =>
            {
                if (string.IsNullOrEmpty(request.FromProgrammingLanguage) || string.IsNullOrEmpty(request.ToProgrammingLanguage) || string.IsNullOrEmpty(request.CodeToConvert))
                    throw new Exception("Bad request");

                var prompt = PromptUtils.GenerateConvertCodePrompt(request);

                var sqsMessage = InstantiateSqsMessageToSerialize(DynamoDBTypes.ConvertCode, prompt);
                var sqsMessageAttributes = SqsUtils.GetSqsMessageAttributes(sqsMessage);
                var sqsMessageGuid = (await SqsPublisher.PublishAsync<GptMessage>(AwsConfiguration.GptWorkItemsTableName ?? "", sqsMessageAttributes, sqsMessage)).MessageId;

                await DynamoDbService.InsertWorkItemRecordIntoDynamoDB(sqsMessage, sqsMessageGuid);
                return new WorkResponse() { WorkGuid = sqsMessageGuid };
            });
        }

        [HttpPost("smart-contract-generator")]
        public async Task<WorkResponse> SmartContractGenerator([FromBody] SmartContractGeneratorRequest request)
        {
            return await GptUtils.ExecuteWithExceptionHandling<WorkResponse>(async () =>
            {
                if (string.IsNullOrEmpty(request.ContractName) || string.IsNullOrEmpty(request.TokenStandard) || string.IsNullOrEmpty(request.WhatDoesItDo))
                    throw new Exception("Bad request");

                var prompt = PromptUtils.GenerateSMContractGeneratorPrompt(request);
                var sqsMessage = InstantiateSqsMessageToSerialize(DynamoDBTypes.SmartContractGenerator, prompt);
                var sqsMessageAttributes = SqsUtils.GetSqsMessageAttributes(sqsMessage);
                var sqsMessageGuid = (await SqsPublisher.PublishAsync<GptMessage>(AwsConfiguration.GptWorkItemsTableName ?? "", sqsMessageAttributes, sqsMessage)).MessageId;

                await DynamoDbService.InsertWorkItemRecordIntoDynamoDB(sqsMessage, sqsMessageGuid);
                return new WorkResponse() { WorkGuid = sqsMessageGuid };
            });
        }
        
        [HttpPost("ui-component-generator")]
        public async Task<WorkResponse> UIComponentGenerator([FromBody] UIComponentGeneratorRequest request)
        {
            return await GptUtils.ExecuteWithExceptionHandling<WorkResponse>(async () =>
            {
                if (string.IsNullOrEmpty(request.NameOfComponent) || string.IsNullOrEmpty(request.PurposeOfComponent))
                    throw new Exception("Bad request");

                var prompt = PromptUtils.GenerateUIComponentGeneratorPrompt(request);
                
                var sqsMessage = InstantiateSqsMessageToSerialize(DynamoDBTypes.UIComponentGenerator, prompt);
                var sqsMessageAttributes = SqsUtils.GetSqsMessageAttributes(sqsMessage);
                var sqsMessageGuid = (await SqsPublisher.PublishAsync<GptMessage>(AwsConfiguration.GptWorkItemsTableName ?? "", sqsMessageAttributes, sqsMessage)).MessageId;

                await DynamoDbService.InsertWorkItemRecordIntoDynamoDB(sqsMessage, sqsMessageGuid);
                return new WorkResponse() { WorkGuid = sqsMessageGuid };
            });
        }

        [HttpPost("text-to-image-generator")]
        public async Task<WorkResponse> TextToImageGenerator([FromBody] TextToImageGeneratorRequest request)
        {
            return await GptUtils.ExecuteWithExceptionHandling<WorkResponse>(async () =>
            {
                if (string.IsNullOrEmpty(request.Concept) || string.IsNullOrEmpty(request.Size))
                    throw new Exception("Bad request");

                var sqsMessage = InstantiateSqsMessageToSerialize(DynamoDBTypes.TextToImage, $"{request.Concept}:{request.Size}");
                var sqsMessageAttributes = SqsUtils.GetSqsMessageAttributes(sqsMessage);
                var sqsMessageGuid = (await SqsPublisher.PublishAsync<GptMessage>(AwsConfiguration.GptWorkItemsTableName ?? "", sqsMessageAttributes, sqsMessage)).MessageId;
                
                await DynamoDbService.InsertWorkItemRecordIntoDynamoDB(sqsMessage, sqsMessageGuid);
                return new WorkResponse() { WorkGuid = sqsMessageGuid };
            });
        }

        [HttpPost("chatbot")]
        public async Task<WorkResponse> Chatbot([FromBody] ChatbotRequest request)
        {
            return await GptUtils.ExecuteWithExceptionHandling<WorkResponse>(async () =>
            {
                if (string.IsNullOrEmpty(request.NewMessage))
                    throw new Exception("Bad request");

                var prompt = PromptUtils.GenerateChatbotMessagePrompt(request);

                var sqsMessage = InstantiateSqsMessageToSerialize(DynamoDBTypes.Chatbot, prompt);
                var sqsMessageAttributes = SqsUtils.GetSqsMessageAttributes(sqsMessage);
                var sqsMessageGuid = (await SqsPublisher.PublishAsync<GptMessage>(AwsConfiguration.GptWorkItemsTableName ?? "", sqsMessageAttributes, sqsMessage)).MessageId;

                await DynamoDbService.InsertWorkItemRecordIntoDynamoDB(sqsMessage, sqsMessageGuid);
                return new WorkResponse() { WorkGuid = sqsMessageGuid };
            });
        }

        private GptMessage InstantiateSqsMessageToSerialize(string messageType, string prompt) => new GptMessage()
        {
            MessageType = messageType,
            Status = (int)GptStatuses.Pending,
            CreatedDate = DateTime.UtcNow.ToString(),
            Prompt = prompt
        };
    }
}