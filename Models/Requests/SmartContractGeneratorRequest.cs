using Newtonsoft.Json;

namespace Utils.IO.Server.Models.Requests
{
    public class SmartContractGeneratorRequest
    {
        public string? Blockchain { get; set; }
        public string? TokenStandard { get; set; }
        public string? ContractName { get; set; }
        public string? WhatDoesItDo { get; set; }
    }
}
