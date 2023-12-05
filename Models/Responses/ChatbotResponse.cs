using Newtonsoft.Json;
using Utils.IO.Server.Utils;

namespace Utils.IO.Server.Models.Responses
{

    public class ChatbotResponse
    {
        public List<ChatbotMessage>? Messages { get; set; }
    }
}
