using System;
using System.Threading.Tasks;
using DBTransferProject.AIServices;
using OpenAI_API;
using OpenAI_API.Completions;
using Microsoft.Extensions.Logging;
using AI.Dev.OpenAI.GPT;


namespace DBTransferProject.AIServices
{
    public class UrgencyDetectionAgent : IAIServiceAgent
    {
        private readonly OpenAIAPI _api;
        private readonly ILogger<UrgencyDetectionAgent> _logger;
        private readonly CostTracker _costTracker;

        private const string SystemMessage = @"
            You are an AI assistant tasked with determining the urgency of customer inquiries.
            Analyze the inquiry and determine if it is a high-priority issue that needs immediate attention.

            Please make sure your response ONLY contains the response in the following JSON format without any additional text or explanations:

            {
              ""Urgency"": ""High"" or ""Low""
            }

            Here is the inquiry to analyze:
            {input}";

        public UrgencyDetectionAgent(OpenAIAPI api, ILogger<UrgencyDetectionAgent> logger, CostTracker costTracker)
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
                    MaxTokens = 100,
                    Temperature = 0.7
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
                return $"{{\"Urgency\":{jsonResponse},\"Cost\":{cost}}}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the urgency detection: {input}", input);
                throw;
            }
        }
    }
}