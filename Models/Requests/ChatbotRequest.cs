using Utils.IO.Server.Utils;

namespace Utils.IO.Server.Models.Requests
{
    public class ChatbotRequest
    {
        public string? NewMessage { get; set; }
        public List<ChatbotMessage>? Messages { get; set; }

    }
}
