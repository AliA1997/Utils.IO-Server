using Newtonsoft.Json;

namespace Utils.IO.Server.Models.Responses
{
    public class ResultsResponse
    {
        [JsonProperty("resultUrl", Required = Required.Always)]
        public string ResultUrl { get; set; }
    }
}
