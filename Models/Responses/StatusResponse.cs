using Newtonsoft.Json;

namespace Utils.IO.Server.Models.Responses
{
    public class StatusResponse
    {
        [JsonProperty("status", Required = Required.Always)]
        public string Status { get; set; }
    }
}
