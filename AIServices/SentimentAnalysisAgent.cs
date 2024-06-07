using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenAI_API;
using OpenAI_API.Completions;
using AI.Dev.OpenAI.GPT;
using Microsoft.Extensions.Logging;

namespace DBTransferProject.AIServices
{
    public class SentimentAnalysisAgent : IAIServiceAgent
    {
        private readonly OpenAIAPI _api;
        private readonly CostTracker _costTracker;
        private readonly ILogger<SentimentAnalysisAgent> _logger;
        private const string SystemMessage = @"
        You are an AI assistant tasked with analyzing the sentiment of customer email messages.
        Please perform a sentiment analysis of the entire conversation and return the result in one word whether 
        the conversation is ""Positive"", ""Negative"", or ""Neutral"".
        
        Here are the messages to analyze:
        {input}
        return the result in one word
        ";
        private static readonly HashSet<string> ValidSentiments = new HashSet<string>
        {
            "Positive",
            "Negative",
            "Neutral"
        };

        public SentimentAnalysisAgent(OpenAIAPI api, CostTracker costTracker, ILogger<SentimentAnalysisAgent> logger)
        {
            _api = api;
            _costTracker = costTracker;
            _logger = logger;
        }

        public async Task<string> ProcessAsync(string input)
        {
            var prompt = SystemMessage.Replace("{input}", input);
            int inputTokens = GPT3Tokenizer.Encode(prompt).Count;
            var sentiment = string.Empty;
            int retryCount = 0;
            const int maxRetries = 3;

            int totalInputTokens = 0;
            int totalOutputTokens = 0;

            while (retryCount < maxRetries && string.IsNullOrEmpty(sentiment))
            {
                try
                {
                    var result = await _api.Completions.CreateCompletionAsync(new CompletionRequest
                    {
                        Prompt = prompt,
                        MaxTokens = 50,
                        Temperature = 0.7
                    });

                    sentiment = result.Completions[0].Text.Trim();
                    sentiment = Regex.Replace(sentiment, "\"", "");

                    if (!ValidSentiments.Contains(sentiment))
                    {
                        _logger.LogWarning("Invalid sentiment detected: {sentiment}", sentiment);
                        sentiment = string.Empty;
                    }
                    else
                    {
                        int outputTokens = GPT3Tokenizer.Encode(sentiment).Count;
                        totalInputTokens += inputTokens;
                        totalOutputTokens += outputTokens;
                    }

                    retryCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in SentimentAnalysisAgent.ProcessAsync method during retry {RetryCount}: {ErrorMessage}", retryCount, ex.Message);
                    retryCount++;
                }
            }

            if (string.IsNullOrEmpty(sentiment))
            {
                var errorMessage = "Failed to determine sentiment after multiple attempts.";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            var modelName = "gpt-3.5-turbo-1106";
            var cost = _costTracker.CalculateCost(modelName, totalInputTokens, totalOutputTokens);

            return $"{{\"Sentiment\":\"{sentiment}\",\"Cost\":{cost}}}";
        }
    }
}