namespace Utils.IO.Server
{
    public class AWSConfiguration
    {
        public string? QueueName { get; set; }
        public string? GptWorkItemsTableName { get; set; }
        public string? GptWorkResultsTableName { get; set; }

        public string? S3BucketName { get; set; }
    }
}
