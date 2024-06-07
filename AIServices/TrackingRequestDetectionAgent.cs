/*************************************************************
*************************************************************
 Class name: TrackingInfoAgent
 Developer: Sam Espinoza
 Date: 2024-06-06
 Explanation: This class represents an AI service agent responsible 
 for determining if a customer email is requesting tracking information 
 using OpenAI's API. It includes methods for processing input messages, 
 validating the response, and tracking costs associated with the API usage.
*************************************************************
*************************************************************/

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Completions;
using AI.Dev.OpenAI.GPT;
using System.Text.Json;

namespace DBTransferProject.AIServices
{
    public class TrackingInfoAgent : IAIServiceAgent
    {
        /*************************************************************
         Fields and Constants
         *************************************************************/
        private readonly OpenAIAPI _api; // Instance of OpenAI API
        private readonly CostTracker _costTracker; // Instance of CostTracker for tracking API costs
        private readonly ILogger<TrackingInfoAgent> _logger; // Logger instance for logging errors and information
        private readonly string _modelName; // Model name

        // Confidence threshold
        private const double ConfidenceThreshold = 0.85;
        private const int MaxRetries = 10;

        // System message template for the AI assistant
        private const string SystemMessage = @"
        You are an AI assistant tasked with determining if a customer email is requesting tracking information.
        Return a JSON object with a binary answer (""Yes"" or ""No"") and a confidence level (0 to 100).

        Return the results in this format:
        {
            ""IsTrackingRequest"": ""Yes"" or ""No"",
            ""ConfidenceLevel"": confidence level
        }

        Here are the messages to analyze:
        {input}";

        /*************************************************************
         Constructor: TrackingInfoAgent
         Developer: Sam Espinoza
         Date: 2024-06-06
         Explanation: Initializes a new instance of the TrackingInfoAgent class 
         with the specified OpenAI API, cost tracker, and logger instances.
         *************************************************************/
        public TrackingInfoAgent(OpenAIAPI api, CostTracker costTracker, ILogger<TrackingInfoAgent> logger, string modelName = "gpt-4-turbo")
        {
            _api = api;
            _costTracker = costTracker;
            _logger = logger;
            _modelName = modelName;
        }

        /*************************************************************
         Method name: ProcessAsync
         Developer: Sam Espinoza
         Date: 2024-06-06
         Explanation: Processes the input message to determine if it is a 
         tracking request. It constructs the prompt, sends it to the OpenAI API, 
         and returns the response along with the cost.
         Throws an InvalidOperationException if the determination fails 
         after multiple attempts.
         *************************************************************/
        public async Task<string> ProcessAsync(string input)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
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
                            MaxTokens = 100,
                            Temperature = 0.3
                        };

                        var result = await _api.Completions.CreateCompletionAsync(completionRequest);
                        fullResponse = result.Completions[0].Text.Trim();
                    }

                    var jsonResponse = ExtractJsonFromResponse(fullResponse);
                    var validatedJsonResponse = ValidateAndCleanResponse(jsonResponse);

                    // Calculate the cost of the API call
                    int inputTokens = GPT3Tokenizer.Encode($"{SystemMessage}\n\n{userMessage}").Count;
                    int outputTokens = GPT3Tokenizer.Encode(fullResponse).Count;
                    var cost = _costTracker.CalculateCost(_modelName, inputTokens, outputTokens);

                    // Log confidence level
                    var jsonObject = JsonDocument.Parse(validatedJsonResponse).RootElement;
                    var isTrackingRequest = jsonObject.GetProperty("IsTrackingRequest").GetString();
                    var confidenceLevel = jsonObject.GetProperty("ConfidenceLevel").GetDouble();
                    _logger.LogInformation("Tracking Request: {isTrackingRequest}, Confidence Level: {confidenceLevel}", isTrackingRequest, confidenceLevel);

                    // Check if the confidence level is above the threshold
                    if (confidenceLevel >= ConfidenceThreshold)
                    {
                        return $"{{\"Result\":{validatedJsonResponse},\"Cost\":{cost}}}";
                    }

                    _logger.LogWarning("Attempt {attempt}: Confidence level {confidenceLevel} below threshold {ConfidenceThreshold}", attempt, confidenceLevel, ConfidenceThreshold);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the input on attempt {attempt}: {input}", attempt, input);
                    if (attempt == MaxRetries)
                    {
                        throw;
                    }
                }
            }

            throw new InvalidOperationException("Failed to determine if the email is requesting tracking information after multiple attempts.");
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

        /*************************************************************
         Method name: ValidateAndCleanResponse
         Developer: Sam Espinoza
         Date: 2024-06-06
         Explanation: Validates and cleans the response to ensure it 
         only contains "Yes" or "No" and the confidence level.
         *************************************************************/
        private string ValidateAndCleanResponse(string jsonResponse)
        {
            try
            {
                var jsonObject = JsonDocument.Parse(jsonResponse).RootElement;
                if (jsonObject.TryGetProperty("IsTrackingRequest", out var isTrackingRequestElement) &&
                    jsonObject.TryGetProperty("ConfidenceLevel", out var confidenceLevelElement))
                {
                    var isTrackingRequest = isTrackingRequestElement.GetString();
                    var confidenceLevel = confidenceLevelElement.GetDouble();

                    if ((isTrackingRequest == "Yes" || isTrackingRequest == "No") && confidenceLevel >= 0 && confidenceLevel <= 100)
                    {
                        return JsonSerializer.Serialize(new
                        {
                            IsTrackingRequest = isTrackingRequest,
                            ConfidenceLevel = confidenceLevel
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid JSON response format: {jsonResponse}", jsonResponse);
            }

            // If no valid JSON is found, return null to indicate invalid response
            return "{}";
        }

        /*************************************************************
         Method name: ExtractConfidenceLevel
         Developer: Sam Espinoza
         Date: 2024-06-06
         Explanation: Extracts the confidence level from the JSON response.
         *************************************************************/
        private double ExtractConfidenceLevel(string responseJson)
        {
            var jsonResponse = JsonDocument.Parse(responseJson).RootElement;
            return jsonResponse.GetProperty("ConfidenceLevel").GetDouble();
        }
    }
}
