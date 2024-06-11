using AI.Dev.OpenAI.GPT;
using AI_RAG_Excelligence.Interfaces;
using DBTransferProject.AIServices;
using Newtonsoft.Json.Linq;
using OpenAI_API.Chat;
using OpenAI_API.Completions;
using OpenAI_API;
using System.Text.RegularExpressions;

public class EntityExtractionAgent : IAIServiceAgent
{
    private readonly OpenAIAPI _api;
    private readonly ILogger<EntityExtractionAgent> _logger;
    private readonly CostTracker _costTracker;
    private readonly string _modelName;
    private readonly IRetrievalService _retrievalService;

    private const string SystemMessage = @"
        You are an AI assistant tasked with extracting key entities from customer email messages.
        Please identify Account Number, Purchase Order Number, Item Number, Order Number, Tracking Numbers, and Carrier (if any).
        Return the results in a structured JSON format without any additional text or explanations:
        {
            ""AccountNumber"": ""Extracted account number (if any, else null)"",
            ""PoNumber"": ""Extracted Purchase Order number (if any, else null)"",
            ""ItemNumber"": ""Extracted item numbers (if any, else null)"",
            ""OrderNumber"": ""Extracted order numbers (if any, else null)"",
            ""TrackingNumber"": ""Extracted tracking number (if any, else null)"",
            ""Carrier"": ""Extracted carrier company (if any, else null)""
        }";

    public EntityExtractionAgent(OpenAIAPI api, ILogger<EntityExtractionAgent> logger, CostTracker costTracker, IRetrievalService retrievalService, string modelName = "gpt-4-turbo")
    {
        _api = api;
        _logger = logger;
        _costTracker = costTracker;
        _modelName = modelName;
        _retrievalService = retrievalService;
    }

    public async Task<string> ProcessAsync(string input)
    {
        try
        {
            // Retrieve relevant documents from the vector database using the RAG system
            var relevantDocuments = await _retrievalService.RetrieveDocumentsAsync(input);

            // Combine the relevant documents into a single string
            var combinedDocuments = string.Join("\n", relevantDocuments.Select(doc => doc.Content));

            // Log the combined documents
            _logger.LogInformation($"Retrieved documents for context: {combinedDocuments}");

            // Create the system message with the documents included
            var extendedSystemMessage = $@"
            {SystemMessage}
            
            Use the following documents as guidelines for identifying entities:
            {combinedDocuments}";

            // Log the combined input
            _logger.LogInformation($"Original Email for analysis: {input}");

            var userMessage = $"Now, please analyze the following email and extract the entities:\n{input}";
            string fullResponse;

            if (_modelName.StartsWith("gpt-4-turbo"))
            {
                var chatRequest = new ChatRequest
                {
                    Model = _modelName,
                    Messages = new[]
                    {
                    new ChatMessage(ChatMessageRole.System, extendedSystemMessage),
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
                    Prompt = $"{extendedSystemMessage}\n\n{userMessage}",
                    MaxTokens = 500,
                    Temperature = 0.8
                };

                var result = await _api.Completions.CreateCompletionAsync(completionRequest);
                fullResponse = result.Completions[0].Text.Trim();
            }

            var jsonResponse = ExtractJsonFromResponse(fullResponse);
            var validatedJsonResponse = ValidateAndCleanEntities(jsonResponse, input);

            // Calculate the cost of the API call
            int inputTokens = GPT3Tokenizer.Encode($"{extendedSystemMessage}\n\n{userMessage}").Count;
            int outputTokens = GPT3Tokenizer.Encode(fullResponse).Count;
            var cost = _costTracker.CalculateCost(_modelName, inputTokens, outputTokens);
            // Return the JSON response and cost as a JSON string
            return $"{{\"Entities\":{validatedJsonResponse},\"Cost\":{cost}}}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while extracting entities from the input: {input}", input);
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

    private string ValidateAndCleanEntities(string jsonResponse, string originalInput)
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
                propertyValue = Regex.Replace(propertyValue, @"[^a-zA-Z0-9\s,]", "");

                // Split the property value into individual entities
                var entities = propertyValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(entity => entity.Trim())
                    .Where(entity => !string.IsNullOrWhiteSpace(entity))
                    .ToList();

                // Check if each entity is present in the original input
                var validEntities = entities.Select(entity =>
                {
                    if (propertyName.Equals("TrackingNumber", StringComparison.OrdinalIgnoreCase))
                    {
                        // Remove any non-numeric characters from the tracking number
                        entity = Regex.Replace(entity, @"[^0-9]", "");
                    }
                    return IsEntityPresentInInput(entity, originalInput) ? entity : null;
                })
                .Where(entity => !string.IsNullOrWhiteSpace(entity))
                .ToList();

                if (validEntities.Any())
                {
                    cleanedJsonObject.Add(propertyName, JArray.FromObject(validEntities));
                }
                else
                {
                    _logger.LogWarning("No valid entities found for property: {propertyName}", propertyName);
                }
            }

            return cleanedJsonObject.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while validating and cleaning entities: {jsonResponse}", jsonResponse);
            return "{}";
        }
    }

    private bool IsEntityPresentInInput(string entity, string input)
    {
        return input.Contains(entity, StringComparison.OrdinalIgnoreCase);
    }
}
