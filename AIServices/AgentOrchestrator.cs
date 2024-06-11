using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using DBTransferProject.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using System;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.Json;

namespace DBTransferProject.AIServices
{
    public class AgentOrchestrator
    {
        // delete validation agent when done
        private readonly ValidationAgent _validationAgent;
        private readonly TrackingInfoAgent _trackingInfoAgent;
        private readonly ILogger<AgentOrchestrator> _logger;
        private readonly CarrierTrackingAgent _carrierTrackingAgent;
        private readonly IServiceProvider _serviceProvider;
        //
        private readonly CategorizationAgent _categorizationAgent;
        private readonly SentimentAnalysisAgent _sentimentAnalysisAgent;
        private readonly EntityExtractionAgent _entityExtractionAgent;
        private readonly ImportantInformationExtractionAgent _importantInformationExtractionAgent;
        private readonly ActionSelectionAgent _actionSelectionAgent;
        public AgentOrchestrator(
            CategorizationAgent categorizationAgent,
            SentimentAnalysisAgent sentimentAnalysisAgent,
            EntityExtractionAgent entityExtractionAgent,
            ImportantInformationExtractionAgent importantInformationExtractionAgent,
            ActionSelectionAgent actionSelectionAgent,
            TrackingInfoAgent trackingInfoAgent,
            // remove this when done v
            ValidationAgent validationAgent,
            ILogger<AgentOrchestrator> logger,
            CarrierTrackingAgent carrierTrackingAgent,
            IServiceProvider serviceProvider)
        {
            _categorizationAgent = categorizationAgent;
            _sentimentAnalysisAgent = sentimentAnalysisAgent;
            _entityExtractionAgent = entityExtractionAgent;
            _importantInformationExtractionAgent = importantInformationExtractionAgent;
            _actionSelectionAgent = actionSelectionAgent;
            _trackingInfoAgent = trackingInfoAgent;
            // remove validation agent once done
            _validationAgent = validationAgent;
            _logger = logger;
            _carrierTrackingAgent = carrierTrackingAgent;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> ProcessEmailAsync(string emailContent)
        {
            try
            {
                var categoryResult = await _categorizationAgent.ProcessAsync(emailContent);
                var sentimentResult = await _sentimentAnalysisAgent.ProcessAsync(emailContent);
                var keywordsResult = await _entityExtractionAgent.ProcessAsync(emailContent);
                var importantInfoResult = await _importantInformationExtractionAgent.ProcessAsync(emailContent);
                //var recommendedActionResult = await GetRecommendedActionResultAsync(emailContent);

                //_logger.LogInformation("Category Result Orchestrator: {categoryResult}", categoryResult);
                //_logger.LogInformation("Sentiment Result: {sentimentResult}", sentimentResult);
                //_logger.LogInformation("Keywords Result: {keywordsResult}", keywordsResult);
                //_logger.LogInformation("Important Info Result: {importantInfoResult}", importantInfoResult);
                //_logger.LogInformation("Recommended Action Result: {recommendedActionResult}", recommendedActionResult);

                double totalCost = 0;

                var aiResponse = new AIResponse
                {
                    Category = JObject.Parse(categoryResult)?["Category"]?.ToString() ?? string.Empty,
                    Sentiment = JObject.Parse(sentimentResult)?["Sentiment"]?.ToString() ?? string.Empty,
                    Keywords = ParseKeywords(keywordsResult),
                    ImportantInformation = ParseImportantInformation(JObject.Parse(importantInfoResult)),
                    Cost = totalCost
                };
             

                // Add costs from each agent's response
                totalCost += JObject.Parse(categoryResult)?["Cost"]?.ToObject<double>() ?? 0.0;
                totalCost += JObject.Parse(sentimentResult)?["Cost"]?.ToObject<double>() ?? 0.0;
                totalCost += JObject.Parse(keywordsResult)?["Cost"]?.ToObject<double>() ?? 0.0;
                totalCost += JObject.Parse(importantInfoResult)?["Cost"]?.ToObject<double>() ?? 0.0;
                //totalCost += JObject.Parse(recommendedActionResult)?["Cost"]?.ToObject<double>() ?? 0.0;

                aiResponse.Cost = totalCost;

                // after email has been processed and the intention of the email has been retrieved 
                aiResponse = await _actionSelectionAgent.ProcessAsync(aiResponse);


                //// Parse the recommended action
                //var recommendedActionJson = JObject.Parse(recommendedActionResult.Trim());
                //var recommendedAction = recommendedActionJson["RecommendedAction"];
                //aiResponse.Action = recommendedAction?["Action"]?.ToString() ?? string.Empty;
                //aiResponse.EmailPrompt = recommendedAction?["Prompt"]?.ToString() ?? string.Empty;

                return JsonConvert.SerializeObject(aiResponse);
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex, "Error parsing AI response JSON");
                throw;
            }
        }

        public async Task<string> GetCategoryResultAsync(string emailContent)
        {
            return await _categorizationAgent.ProcessAsync(emailContent);
        }
        public async Task<string> GetRecommendedActionResultAsync(string emailContent)
        {
            return await _validationAgent.ValidateAndGetResponseAsync(() => _validationAgent.RecommendedActionAgent.ProcessAsync(emailContent));
        }

        private Keywords ParseKeywords(string keywordsResult)
        {
            try
            {
                var entities = JObject.Parse(keywordsResult)["Entities"];
                return new Keywords
                {
                    AccountNumber = ConvertToList(entities?["AccountNumber"]),
                    PoNumber = ConvertToList(entities?["PoNumber"]),
                    ItemNumber = ConvertToList(entities?["ItemNumber"]),
                    OrderNumber = ConvertToList(entities?["OrderNumber"]),
                    TrackingNumber = ConvertToList(entities?["TrackingNumber"]),
                    Carrier = ConvertToList(entities?["Carrier"])
                };
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex, "Error parsing Keywords JSON");
                throw;
            }
        }
        private ImportantInformation ParseImportantInformation(JObject importantInfoResult)
        {
            var info = importantInfoResult["ImportantInformation"];
            return new ImportantInformation
            {
                CustomerName = info?["CustomerName"]?.ToString() ?? string.Empty,
                CustomerPhone = info?["CustomerPhone"]?.ToString() ?? string.Empty,
                CustomerEmail = info?["CustomerEmail"]?.ToString() ?? string.Empty,
                CustomerAddress = info?["CustomerAddress"]?.ToString() ?? string.Empty,
                Organization = info?["Organization"]?.ToString() ?? string.Empty
            };
        }

        private List<string> ConvertToList(JToken token)
        {
            if (token == null)
            {
                return new List<string>();
            }

            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<string>>();
            }
            else if (token.Type == JTokenType.String)
            {
                return new List<string> { token.ToString() };
            }

            return new List<string>();
        }

        public async Task<(bool IsTrackingRequest, double ConfidenceLevel)> CheckForTrackingRequestAsync(string emailContent)
        {
            var trackingResultJson = await _trackingInfoAgent.ProcessAsync(emailContent);
            var trackingResult = JsonDocument.Parse(trackingResultJson).RootElement;

            if (trackingResult.TryGetProperty("Result", out var resultElement))
            {
                if (resultElement.TryGetProperty("IsTrackingRequest", out var isTrackingRequestElement) &&
                    resultElement.TryGetProperty("ConfidenceLevel", out var confidenceLevelElement))
                {
                    var isTrackingRequest = isTrackingRequestElement.GetString() == "Yes";
                    var confidenceLevel = confidenceLevelElement.GetDouble();
                    return (isTrackingRequest, confidenceLevel);
                }
            }

            throw new KeyNotFoundException("The necessary keys were not present in the JSON response.");
        }
    }
}
