using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Completions;
using Microsoft.Extensions.Logging;
using AI.Dev.OpenAI.GPT;
using Newtonsoft.Json.Linq;

namespace DBTransferProject.AIServices
{
    public class ImportantInformationExtractionAgent : IAIServiceAgent
    {
        private readonly OpenAIAPI _api;
        private readonly ILogger<ImportantInformationExtractionAgent> _logger;
        private readonly CostTracker _costTracker;
        private readonly string _modelName;

        private const string SystemMessage = @"
            You are an AI assistant tasked with extracting important customer information from email messages.
            Please extract the following information if explicitly available and return the results in a structured JSON format without any additional text or explanations:
            Do not generate or speculate any additional information. Ensure that all provided information is accurate and complete.
            {
                ""CustomerName"": ""Extracted customer name (if any, else null)"",
                ""CustomerPhone"": ""Extracted customer phone number (if any, else null)"",
                ""CustomerEmail"": ""Extracted customer email (if any, else null)"",
                ""CustomerAddress"": ""Extracted customer address (if any, else null)"",
                ""Organization"": ""Extracted organization (if any, else null)""
            }

            Important: Customer phone numbers and addresses are crucial. Ensure to check carefully for any phone numbers and addresses in various formats.
            Also, check for customer information within the signature block at the end of the email body.";

        public ImportantInformationExtractionAgent(OpenAIAPI api, ILogger<ImportantInformationExtractionAgent> logger, CostTracker costTracker, string modelName = "gpt-4-turbo")
        {
            _api = api;
            _logger = logger;
            _costTracker = costTracker;
            _modelName = modelName;
        }

        public async Task<string> ProcessAsync(string input)
        {
            try
            {
                var userMessage = $"Here are the messages to analyze:\n{input}";
                string fullResponse;

                if (_modelName.StartsWith("gpt-4-turbo"))
                {
                    var chatRequest = new ChatRequest
                    {
                        Model = _modelName,
                        Messages = new[]
                        {
                            new ChatMessage(ChatMessageRole.System, SystemMessage),
                            new ChatMessage(ChatMessageRole.User, userMessage)
                        }
                    };

                    var chatResult = await _api.Chat.CreateChatCompletionAsync(chatRequest);
                    fullResponse = chatResult.Choices[0].Message.Content.Trim();
                }
                else
                {
                    var completionRequest = new CompletionRequest
                    {
                        Model = _modelName,
                        Prompt = $"{SystemMessage}\n\n{userMessage}",
                        MaxTokens = 200,
                        Temperature = 0.8
                    };

                    var result = await _api.Completions.CreateCompletionAsync(completionRequest);
                    fullResponse = result.Completions[0].Text.Trim();
                }

                var jsonResponse = ExtractJsonFromResponse(fullResponse);
                var validatedJsonResponse = ValidateAndCleanImportantInformation(jsonResponse, input);

                // Calculate the cost of the API call
                int inputTokens = GPT3Tokenizer.Encode($"{SystemMessage}\n\n{userMessage}").Count;
                int outputTokens = GPT3Tokenizer.Encode(fullResponse).Count;
                var cost = _costTracker.CalculateCost(_modelName, inputTokens, outputTokens);

                // Return the JSON response and cost as a JSON string
                return $"{{\"ImportantInformation\":{validatedJsonResponse},\"Cost\":{cost}}}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while extracting important information from the input: {input}", input);
                throw;
            }
        }

        private string ExtractJsonFromResponse(string response)
        {
            var jsonStartIndex = response.IndexOf("{");
            var jsonEndIndex = response.LastIndexOf("}");

            if (jsonStartIndex >= 0 && jsonEndIndex >= 0 && jsonEndIndex >= jsonStartIndex)
            {
                return response.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);
            }
            else
            {
                _logger.LogWarning("No valid JSON found in the AI response: {fullResponse}", response);
                return "{}";
            }
        }

        private string ValidateAndCleanImportantInformation(string jsonResponse, string originalInput)
        {
            try
            {
                var jsonObject = JObject.Parse(jsonResponse);
                var cleanedJsonObject = new JObject();

                foreach (var property in jsonObject.Properties())
                {
                    var propertyName = property.Name;
                    var propertyValue = property.Value.ToString();

                    // Remove special characters and unwanted formatting
                    propertyValue = Regex.Replace(propertyValue, @"[^a-zA-Z0-9\s,@.]", "");

                    // Check if the extracted information is explicitly present in the original input
                    if (IsInformationExplicitlyPresent(propertyValue, originalInput))
                    {
                        cleanedJsonObject.Add(propertyName, propertyValue);
                    }
                    else
                    {
                        _logger.LogWarning("Speculated or additional information detected for property: {propertyName} {propertyValue}", propertyName, propertyValue);
                    }
                }

                return cleanedJsonObject.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while validating and cleaning important information: {jsonResponse}", jsonResponse);
                return "{}";
            }
        }

        private bool IsInformationExplicitlyPresent(string information, string input)
        {
            return input.Contains(information, StringComparison.OrdinalIgnoreCase);
        }
    }
}
