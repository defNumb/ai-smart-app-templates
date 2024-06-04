using System;
using System.Threading.Tasks;
using OpenAI_API;
using OpenAI_API.Completions;
using Microsoft.Extensions.Logging;
using AI.Dev.OpenAI.GPT;
namespace DBTransferProject.AIServices
{
    public class ImportantInformationExtractionAgent : IAIServiceAgent
    {
        private readonly OpenAIAPI _api;
        private readonly ILogger<ImportantInformationExtractionAgent> _logger;
        private readonly CostTracker _costTracker;

        private const string SystemMessage = @"
        You are an AI assistant tasked with extracting important customer information from email messages.
        Please extract the following information if explicitly available and return the results in a structured JSON format without any additional text or explanations:
        Do not generate or speculate any additional information.

        {
            ""CustomerName"": ""Extracted customer name (if any, else null)"",
            ""CustomerPhone"": ""Extracted customer phone number (if any, else null)"",
            ""CustomerEmail"": ""Extracted customer email (if any, else null)"",
            ""CustomerAddress"": ""Extracted customer address (if any, else null)"",
            ""Organization"": ""Extracted organization (if any, else null)""
        }

        Here are the messages to analyze:
        {input}";

        public ImportantInformationExtractionAgent(OpenAIAPI api, ILogger<ImportantInformationExtractionAgent> logger, CostTracker costTracker)
        {
            _api = api;
            _logger = logger;
            _costTracker = costTracker;
        }

        public async Task<string> ProcessAsync(string input)
        {
            try
            {
                var prompt = SystemMessage.Replace("{input}", input);
                var result = await _api.Completions.CreateCompletionAsync(new CompletionRequest
                {
                    Prompt = prompt,
                    MaxTokens = 200,
                    Temperature = 0.8
                });

                var fullResponse = result.Completions[0].Text.Trim();
                var jsonStartIndex = fullResponse.IndexOf("{");
                var jsonEndIndex = fullResponse.LastIndexOf("}");

                string jsonResponse;
                if (jsonStartIndex >= 0 && jsonEndIndex >= 0 && jsonEndIndex >= jsonStartIndex)
                {
                    jsonResponse = fullResponse.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);
                }
                else
                {
                    // Handle the case when no valid JSON is found in the response
                    _logger.LogWarning("No valid JSON found in the AI response: {fullResponse}", fullResponse);
                    jsonResponse = "{}";
                }

                // Calculate the cost of the API call
                var modelName = "gpt-3.5-turbo-0125"; // Replace with the actual model name if different
                                                      // Tokenize the input prompt
                int inputTokens = GPT3Tokenizer.Encode(prompt).Count;
                // Tokenize the output text
                int outputTokens = GPT3Tokenizer.Encode(result.Completions[0].Text).Count;
                var cost = _costTracker.CalculateCost(modelName, inputTokens, outputTokens);

                // Return the JSON response and cost as a JSON string
                return $"{{\"ImportantInformation\":{jsonResponse},\"Cost\":{cost}}}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while extracting important information from the input: {input}", input);
                throw;
            }
        }
    }
}