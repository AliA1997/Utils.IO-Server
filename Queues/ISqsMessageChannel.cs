using Amazon.SQS.Model;
using System.Threading.Channels;

namespace Utils.IO.Server.Queues
{
	public interface ISqsMessageChannel
	{
		ChannelReader<Message> Reader { get; }
		Task WriteMessagesAsync(IList<Message> messages, CancellationToken cancellationToken = default);
		void CompleteWriter(Exception? ex = null);
		bool TryCompleteWriter(Exception? ex = null);
	}
}
