using Amazon.DynamoDBv2.DataModel;

namespace Utils.IO.Server.Models.Entity
{
    [DynamoDBTable("gpt-work-items")]
    public class GptWorkItem
    {
        [DynamoDBRangeKey("messageType")]
        public string MessageType { get; set; } = String.Empty;
        [DynamoDBHashKey("messageId")]
        public string MessageId { get; set; } = String.Empty;
        [DynamoDBGlobalSecondaryIndexHashKey("status")]
        public string Status { get; set; } = String.Empty;
    }
}
