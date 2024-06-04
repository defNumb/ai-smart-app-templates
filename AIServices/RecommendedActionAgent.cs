using System;
using System.Threading.Tasks;
using OpenAI_API;
using OpenAI_API.Completions;
using Microsoft.Extensions.Logging;
using AI.Dev.OpenAI.GPT;
namespace DBTransferProject.AIServices
{
    public class RecommendedActionAgent : IAIServiceAgent
    {
        private readonly OpenAIAPI _api;
        private readonly ILogger<RecommendedActionAgent> _logger;
        private readonly CostTracker _costTracker;
        private const string SystemMessage = @"
                    You are an AI assistant tasked with recommending actions based on customer inquiries.
                    Analyze the provided JSON and determine if an email response is necessary or if the conversation should be delegated to a human customer representative.

                    If an email response is necessary, generate a prompt for the next AI agent to generate the appropriate email response. Make sure to give proper
                    context for the AI agent to be able to generate the correct email response.
                    If delegation to a human is required, simply indicate that the conversation should be delegated in the Action.

                    Please make sure your response ONLY contains the response in the following JSON format without any additional text or explanations:
                    {
                      ""Action"": ""EmailResponse"" or ""DelegateToHuman"",
                      ""Prompt"": ""Generated prompt for email response"" (only if Action is EmailResponse)
                    }

                    Here is the inquiry to analyze:
                    {input}";


        public RecommendedActionAgent(OpenAIAPI api, ILogger<RecommendedActionAgent> logger, CostTracker costTracker)
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
                return $"{{\"RecommendedAction\":{jsonResponse},\"Cost\":{cost}}}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the inquiry: {input}", input);
                throw;
            }
        }
    }
}