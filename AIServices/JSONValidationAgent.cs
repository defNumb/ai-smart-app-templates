using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DBTransferProject.AIServices
{
    public class ValidationAgent
    {
        public CategorizationAgent CategorizationAgent { get; }
        public SentimentAnalysisAgent SentimentAnalysisAgent { get; }
        public EntityExtractionAgent EntityExtractionAgent { get; }
        public ImportantInformationExtractionAgent ImportantInformationExtractionAgent { get; }
        public RecommendedActionAgent RecommendedActionAgent { get; }
        private readonly ILogger<ValidationAgent> _logger;

        public ValidationAgent(
            CategorizationAgent categorizationAgent,
            SentimentAnalysisAgent sentimentAnalysisAgent,
            EntityExtractionAgent entityExtractionAgent,
            ImportantInformationExtractionAgent importantInfoAgent,
            RecommendedActionAgent recommendedActionAgent,
            ILogger<ValidationAgent> logger)
        {
            CategorizationAgent = categorizationAgent;
            SentimentAnalysisAgent = sentimentAnalysisAgent;
            EntityExtractionAgent = entityExtractionAgent;
            ImportantInformationExtractionAgent = importantInfoAgent;
            RecommendedActionAgent = recommendedActionAgent;
            _logger = logger;
        }

        public async Task<string> ValidateAndGetResponseAsync(Func<Task<string>> agentTask)
        {
            string response = null;
            bool isValid = false;

            while (!isValid)
            {
                response = await agentTask();
                response = response?.Trim();

                _logger.LogInformation("Raw Agent Response: {response}", response);

                // Check if the response is a valid JSON and properly formatted
                isValid = IsValidJson(response) && !HasExtraBraces(response);

                if (!isValid)
                {
                    _logger.LogWarning("Invalid JSON response. Requesting re-analysis...");
                }
            }

            return response;
        }

        private bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    _logger.LogError(jex, "JSONValidationAgent - Invalid JSON format");
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    _logger.LogError(ex, "JSONValidationAgent - error during JSON validation");
                    return false;
                }
            }
            return false;
        }

        private bool HasExtraBraces(string strInput)
        {
            // Ensure it doesn't have extra braces
            return strInput.Contains("{{") || strInput.Contains("}}");
        }
    }
}
