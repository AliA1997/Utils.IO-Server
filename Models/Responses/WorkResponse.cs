using Newtonsoft.Json;

namespace Utils.IO.Server.Models.Responses
{
    public class WorkResponse
    {
        [JsonProperty("workGuid", Required = Required.Always)]
        public string WorkGuid { get; set; }
    }
}
