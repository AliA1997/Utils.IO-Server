using Utils.IO.Server.Models.Requests;

namespace Utils.IO.Server.Utils
{
    public class DynamoDBTypes
    {
        public const string ArticleSummarizer = "Article Summarizer";
        public const string ConvertCode = "Convert Code";
        public const string ParagraphGenerator = "Paragraph Generator";
        public const string SmartContractGenerator = "Smart Contract Generator";
        public const string UIComponentGenerator = "UI Component Generator";
        public const string TextToImage = "Text To Image Generator";
        public const string Chatbot = "Chatbot";
    }
    public class DynamoDBStatus
    {
        public const string Pending = "Pending";
        public const string Error = "Error";
        public const string Finished = "Finished";
    }
    public class ChatbotRoles
    {
        public const string Assistant = "assistant";
        public const string System = "system";
        public const string User = "user";
    }

    public class ChatbotMessage
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
    }

    public static class PromptUtils
    {
        private static List<string> Web3Pls = new List<string>() { "Ethereum", "Polygon" };
        public static string GenerateArticleSummarizerPrompt(string inputText)
        {
            return $"\"You are an assistant helping to summarize this article based on a prompt.\n" +
                    "Use this format, replacing text in brackets with the result. Do not include\n" +
                    "the brackets in the output:" + "\n\n Summary:\n" + "[Three Paragraph summary of the article]\n\"\n"
                    + inputText;
        }
        public static string GenerateCodeConvertPrompt(string fromPL, string toPL, string codeToConvert)
        {
            return $"\"You are a programmer tasked with converting this code from {fromPL} to {toPL}:\n" +
                    $"{codeToConvert}\n" +
                    "Here are some objects you will need use for this script:\n\n" + "Tasks:\n" +
                    $"1. Convert code from {fromPL} to {toPL}.\n" +
                    "2. Add comments explaining what the code is doing \n\"\n\n" +
                    "Convert Code\n";
        }

        public static string GenerateParagraphGeneratorPrompt(string topic)
        {
            return $"\"You are an assistant helping to generate a paragraph based on a prompt.\n" +
                    "Use this format, replacing text in brackets with the result. Do not include\n" +
                    "the brackets in the output:" + "\n\n Summary:\n " + "[One Paragraph based on prompt]\n\"\n"
                    + topic;
        }

        public static string GenerateConvertCodePrompt(ConvertCodeRequest request)
        {
            return $"\"\"\"You are a programmer tasked with converting this code from {request.FromProgrammingLanguage} to {request.ToProgrammingLanguage}:\n" +
                    $"{request.CodeToConvert}\n" +
                    "Here are some objects you will need to use for this script:\n\n" +
                    $"Tasks:\n 1. Convert code from {request.FromProgrammingLanguage} to ${request.ToProgrammingLanguage}.\n" +
                    $"2. Add comments explaining what the code is doing \\n\"\"\"\\n\\n\n Convert code\n";
        }

        public static string GenerateSMContractGeneratorPrompt(SmartContractGeneratorRequest req)
        {
            var pl = Web3Pls.Any(pl => pl == req.Blockchain) ? "Solidity" : "Rust";
            return $"\"You are a programmer tasked with writing a smart contract that built for the {req.Blockchain} blockchain in" +
                    $"{pl}\n" + "Here are some objects you will need use for this script:\n\n" +
                    "Tasks:\n" + $"1. Write a smart contract that is named {req.ContractName}.\n" +
                    $"2. Write a smart contract that based off the {req.TokenStandard} token standard.\n" +
                    $"3. This contract would be responsible for doing {req.WhatDoesItDo} \n\"\n\n\n" +
                    "Generate Smart Contract\n";
        }

        public static string GenerateUIComponentGeneratorPrompt(UIComponentGeneratorRequest req)
        {
            var uiLibraryUsed = !string.IsNullOrEmpty(req.UILibraryUsed)
                                ? $"You are a programmer tasked with writing a UI component that is built for {req.WebFrameworkUsed} in {req.UILibraryUsed} UI Library.\n"
                                : $"You are a programmer tasked with writing a UI component that is built for {req.WebFrameworkUsed}.\n";
            var howIsComponentIsStyled = !string.IsNullOrEmpty(req.HowIsComponentIsStyled)
                                        ? $"This component is styled using {req.HowIsComponentIsStyled}.\n"
                                        : $"";
            var web3FeatureUsed = !string.IsNullOrEmpty(req.Web3WalletFeatureUsed)
                                    ? $"3. This component integrates with {req.Web3WalletFeatureUsed}.\n"
                                    : "";
            var result = uiLibraryUsed + howIsComponentIsStyled +
                        $"Here are some objects you will need use for this script:\n\n" +
                        "Tasks:\n" + $"1. The name of the component would be ${req.NameOfComponent}.\n" +
                        $"2. The component would be responsible for ${req.PurposeOfComponent}.\n";

            if (!string.IsNullOrEmpty(web3FeatureUsed)) result += web3FeatureUsed;

            result += "}\n\"\n\n\n" + "Generate UI Component \n";

            return result;
        }

        public static string GenerateChatbotMessagePrompt(ChatbotRequest req)
        {
            return $"\"\"\"You are a using asking this prompt: {req.NewMessage}\"\"\".\r\n";
        }
    }
}
