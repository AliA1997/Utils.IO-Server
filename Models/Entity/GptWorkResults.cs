using Amazon.DynamoDBv2.DataModel;

namespace Utils.IO.Server.Models.Entity
{
    [DynamoDBTable("gpt-work-results")]
    public class GptWorkResults
    {
        [DynamoDBRangeKey("resultUrl")]
        public string? ResultUrl { get; set; }
        [DynamoDBHashKey("workItemId")]
        public string? WorkItemId { get; set; }
        [DynamoDBGlobalSecondaryIndexHashKey("resultId")]
        public string? ResultId { get; set; }
    }
}
