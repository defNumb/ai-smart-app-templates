/*************************************************************
*************************************************************
 Class name: CategorizationAgent
 Developer: Sam Espinoza
 Date: 2024-05-31
 Explanation: This class represents an AI service agent responsible 
 for categorizing customer email messages using OpenAI's API. It 
 includes methods for processing input messages, validating and 
 cleaning the response, and tracking costs associated with the 
 API usage.
*************************************************************
*************************************************************/

using System.Threading.Tasks;
using Microsoft.Graph.Models.Security;
using OpenAI_API;
using OpenAI_API.Completions;
using AI.Dev.OpenAI.GPT;

namespace DBTransferProject.AIServices
{
    public class CategorizationAgent : IAIServiceAgent
    {
        /*************************************************************
         Fields and Constants
         *************************************************************/
        private readonly OpenAIAPI _api; // Instance of OpenAI API
        private readonly CostTracker _costTracker; // Instance of CostTracker for tracking API costs
        private readonly ILogger<CategorizationAgent> _logger; // Logger instance for logging errors and information

        // Set of valid categories for email messages
        private static readonly HashSet<string> ValidCategories = new HashSet<string>
                {
                    "Order Inquiry",
                    "Tracking Information",
                    "Shipping and Delivery",
                    "Product Information",
                    "Returns and Refunds",
                    "Billing and Payments",
                    "Technical Support",
                    "Account Management",
                    "Feedback and Complaints",
                    "Promotions and Discounts",
                    "General Inquiry"
                };

        // System message template for the AI assistant
        private const string SystemMessage = @"
        You are an AI assistant tasked with categorizing customer email messages.
        Please return the category of the conversation based on the context between the following categories and RETURN ONLY one of the category name
        from the following categories:

        - Order Inquiry
        - Tracking Information
        - Shipping and Delivery
        - Product Information
        - Returns and Refunds
        - Billing and Payments
        - Technical Support
        - Account Management
        - Feedback and Complaints
        - Promotions and Discounts
        - General Inquiry
        
        Return ONLY the category from the list provided without any other information or explanations.

        Here are the messages to analyze:
        {input}";

        /*************************************************************
         Constructor: CategorizationAgent
         Developer: Sam Espinoza
         Date: 2024-05-31
         Explanation: Initializes a new instance of the CategorizationAgent class 
         with the specified OpenAI API, cost tracker, and logger instances.
         *************************************************************/
        public CategorizationAgent(OpenAIAPI api, CostTracker costTracker, ILogger<CategorizationAgent> logger)
        {
            _api = api;
            _costTracker = costTracker;
            _logger = logger;
        }

        /*************************************************************
         Method name: ProcessAsync
         Developer: Sam Espinoza
         Date: 2024-05-31
         Explanation: Processes the input message to categorize it. It constructs
         the prompt, sends it to the OpenAI API, retries if necessary, and returns
         the categorized response along with the cost. Throws an InvalidOperationException
         if categorization fails after multiple attempts.
         *************************************************************/
        public async Task<string> ProcessAsync(string input)
        {
            var prompt = SystemMessage.Replace("{input}", input);
            int inputTokens = GPT3Tokenizer.Encode(prompt).Count;
            string category = null;
            int retryCount = 0;
            const int maxRetries = 3;

            int totalInputTokens = 0;
            int totalOutputTokens = 0;

            while (retryCount < maxRetries && category == null)
            {
                try
                {
                    var result = await _api.Completions.CreateCompletionAsync(new CompletionRequest
                    {
                        Prompt = prompt,
                        MaxTokens = 100,
                        Temperature = 0.7
                    });

                    var response = result.Completions[0].Text.Trim();
                    category = ValidateAndCleanResponse(response);

                    int outputTokens = GPT3Tokenizer.Encode(response).Count;
                    totalInputTokens += inputTokens;
                    totalOutputTokens += outputTokens;

                    retryCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in CategorizationAgent.ProcessAsync method during retry {RetryCount}: {ErrorMessage}", retryCount, ex.Message);
                    retryCount++;
                }
            }

            if (category == null)
            {
                var errorMessage = "Failed to categorize message after multiple attempts.";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            var modelName = "gpt-3.5-turbo-1106";
            var cost = _costTracker.CalculateCost(modelName, totalInputTokens, totalOutputTokens);

            return $"{{\"Category\":\"{category}\",\"Cost\":{cost}}}";
        }

        /*************************************************************
         Method name: ValidateAndCleanResponse
         Developer: Sam Espinoza
         Date: 2024-05-31
         Explanation: Validates the response by checking if it contains 
         any of the valid categories. If a valid category is found, it 
         returns the category, ensuring no extra text is included. If no 
         valid category is found, it returns null to indicate an invalid response.
         *************************************************************/
        private string ValidateAndCleanResponse(string response)
        {
            foreach (var category in ValidCategories)
            {
                if (response.Contains(category))
                {
                    // Return the category, ensuring no extra text is included
                    return category;
                }
            }

            // If no valid category is found, return null to indicate invalid response
            return null;
        }
    }
}
