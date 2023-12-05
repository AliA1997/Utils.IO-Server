using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Utils.IO.Server.Models.Entity;
using Utils.IO.Server.Models.Responses;
using Utils.IO.Server.Services;

namespace Utils.IO.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PollingController : Controller
    {
        private readonly ILogger<PollingController> Logger;
        private IAmazonDynamoDB DynamoDbClient { get; set; }

        public PollingController(
            ILogger<PollingController> logger,
            IAmazonDynamoDB dynamoDbClient)
        {
            Logger = logger;
            DynamoDbClient = dynamoDbClient;
        }

        [HttpGet("status/{gptId}")]
        public async Task<StatusResponse> PollForStatus(string gptId)
        {
            var gptWorkItem = await GetDynamoDBRecord<GptWorkItem>(gptId);

            return new StatusResponse() { Status = gptWorkItem != null ? gptWorkItem.Status : "" };
        }

        [HttpGet("results/{gptId}")]
        public async Task<ResultsResponse> PollForResultUrl(string gptId)
        {
            var gptResultItem = await GetDynamoDBRecord<GptWorkResults>(gptId);

            return new ResultsResponse() { ResultUrl = gptResultItem != null ? gptResultItem.ResultUrl! : "" };
        }
        
        private async Task<T?> GetDynamoDBRecord<T>(string id)
        {
            using var dynamoDbContext = new DynamoDBContext(DynamoDbClient);

            var item = (await dynamoDbContext.QueryAsync<T>(id).GetRemainingAsync()).FirstOrDefault();

            return item;
        }
    }
}
