namespace Utils.IO.Server.Models.Requests
{
    public class ConvertCodeRequest
    {
        public string? CodeToConvert { get; set; }
        public string? FromProgrammingLanguage { get; set; }
        public string? ToProgrammingLanguage { get; set; }
    }
}
