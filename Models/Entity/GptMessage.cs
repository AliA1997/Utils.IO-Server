using Newtonsoft.Json;

namespace Utils.IO.Server.Models.Entity
{
   
    public interface IMessage
    {
        public string MessageType { get; set; }
    }

    public class GptMessage: IMessage
    {
        [JsonProperty("messageType", Required = Required.Default)]
        public string MessageType { get; set; } = String.Empty;
        [JsonProperty("status", Required = Required.Default)]
        public int Status { get; set; }
        [JsonProperty("createdDate", Required = Required.Default)]
        public string CreatedDate { get; set; } = String.Empty;
        [JsonProperty("prompt", Required = Required.Default)]
        public string Prompt { get; set; } = String.Empty;
    }
}
