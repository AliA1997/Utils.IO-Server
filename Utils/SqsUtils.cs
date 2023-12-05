using Amazon.SQS.Model;
using Utils.IO.Server.Models.Entity;

namespace Utils.IO.Server.Utils
{
    public static class SqsUtils
    {


        public static Dictionary<string, MessageAttributeValue> GetSqsMessageAttributes(GptMessage message) => new Dictionary<string, MessageAttributeValue>()
        {
            {
                nameof(IMessage.MessageType),
                new MessageAttributeValue
                {
                    StringValue = message.MessageType,
                    DataType = "String"
                }
            },
            {
                "status",
                new MessageAttributeValue
                {
                    StringValue = message.Status.ToString(),
                    DataType = "Number"
                }
            },
            {
                "prompt",
                new MessageAttributeValue
                {
                    StringValue = message.Prompt,
                    DataType = "String"
                }
            },
            {
                "timestamp",
                new MessageAttributeValue
                {
                    StringValue = message.CreatedDate,
                    DataType = "String"
                }
            },
            {
                "version",
                new MessageAttributeValue
                {
                    StringValue = "v1",
                    DataType = "String"
                }
            }
        };
    }
}
