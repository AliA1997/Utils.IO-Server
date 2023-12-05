using Amazon.SQS.Model;

namespace Utils.IO.Server.Queues
{
    public interface ISqsMessageQueue
    {
        Task<ReceiveMessageResponse> ReceiveMessageAsync(
            ReceiveMessageRequest request,
            CancellationToken cancellationToken = default);
    }
}

