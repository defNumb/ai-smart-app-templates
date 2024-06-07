using System;
using System.Collections.Generic;

namespace DBTransferProject.AIServices
{
    public class CostTracker
    {
        private const double Gpt35TurboInputCostPerMillion = 1.00; // $1.00 per 1M tokens
        private const double Gpt35TurboOutputCostPerMillion = 2.00; // $2.00 per 1M tokens
        private const double Gpt4TurboInputCostPerMillion = 10.00; // $10.00 per 1M tokens
        private const double Gpt4TurboOutputCostPerMillion = 30.00; // $30.00 per 1M tokens

        private readonly Dictionary<string, (double inputCost, double outputCost)> _pricing = new()
        {
            { "gpt-3.5-turbo-1106", (Gpt35TurboInputCostPerMillion, Gpt35TurboOutputCostPerMillion) },
            { "gpt-4-turbo", (Gpt4TurboInputCostPerMillion, Gpt4TurboOutputCostPerMillion) }
        };

        public double CalculateCost(string modelName, int inputTokens, int outputTokens)
        {
            if (_pricing.TryGetValue(modelName, out var costs))
            {
                double inputCost = (inputTokens / 1_000_000.0) * costs.inputCost;
                double outputCost = (outputTokens / 1_000_000.0) * costs.outputCost;
                return inputCost + outputCost;
            }
            else
            {
                throw new ArgumentException($"Model pricing not found for model: {modelName}");
            }
        }
    }
}
