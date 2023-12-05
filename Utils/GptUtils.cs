using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;
using OpenAI.ObjectModels.RequestModels;
using Utils.IO.Server.Models;
using Utils.IO.Server.Models.Entity;
using Utils.IO.Server.Models.Requests;

namespace Utils.IO.Server.Utils
{

    public enum GptStatuses
    {
        Pending = 0,
        Successful = 1,
        NotReceived = 2,
        Error = 3,
    }
    public class TextToImageArgs
    {
        public TextToImageArgs(string concept, string size)
        {
            Concept = concept;
            Size = size;
        }

        public string Concept { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
    }

    public class CreateChatCompletionArgs
    {
        public CreateChatCompletionArgs(string prompt, double temperature, int maxTokens)
        {
            Prompt = prompt;
            Temperature = temperature;
            MaxTokens = maxTokens;
        }

        public string Prompt { get; set; } = String.Empty;
        public double Temperature { get; set; } = 0.0;
        public double MaxTokens { get; set; } = 0.0;
    }

    public static class GptUtils
    {
        
        public static async Task<T> ExecuteWithExceptionHandling<T>(Func<Task<T>> action)
        {
            try
            {
                return await action().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static ImageCreateRequest CreateTextToImageRequest(TextToImageArgs args)
        {
            return new ImageCreateRequest()
            {
                Prompt = args.Concept,
                N = 1,
                Size = args.Size,
                ResponseFormat = "url"
            };
        }

        public static ChatCompletionCreateRequest CreateChatCompletionRequest(CreateChatCompletionArgs args)
        {
            return new ChatCompletionCreateRequest()
            {
                Model = "gpt-3.5-turbo",
                Messages = new List<ChatMessage>
                {
                    new ChatMessage("assistant", args.Prompt)
                },
                Temperature = (float)args.Temperature,
                MaxTokens = (int)args.MaxTokens,
                TopP = (float)args.Temperature,
                FrequencyPenalty = (float)0.0,
                PresencePenalty = (float)0.0
            };
        }

        public static ChatCompletionCreateRequest CreateChatCompletionRequestBasedOnType(GptMessage message)
        {
            CreateChatCompletionArgs args = new CreateChatCompletionArgs(message.Prompt, 0.5, 1096);
            if (message.MessageType == DynamoDBTypes.ParagraphGenerator)
                args = new CreateChatCompletionArgs(message.Prompt, 0.5, 750);
            if (message.MessageType == DynamoDBTypes.ConvertCode)
                args = new CreateChatCompletionArgs(message.Prompt, 0, 1096);
            if (message.MessageType == DynamoDBTypes.SmartContractGenerator)
                args = new CreateChatCompletionArgs(message.Prompt, 1.0, 2200);
            if (message.MessageType == DynamoDBTypes.ConvertCode)
                args = new CreateChatCompletionArgs(message.Prompt, 0.0, 1096);

            return CreateChatCompletionRequest(args);
        }
    }
}
