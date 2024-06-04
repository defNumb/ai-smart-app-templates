using System;

namespace DBTransferProject.AIServices
{
    public class CostTracker
    {
        private const double Gpt35TurboInputCostPerMillion = 0.50;
        private const double Gpt35TurboOutputCostPerMillion = 1.50;
        private const double Gpt35TurboInstructInputCostPerMillion = 1.50;
        private const double Gpt35TurboInstructOutputCostPerMillion = 2.00;

        private readonly Dictionary<string, (double inputCost, double outputCost)> _pricing = new()
        {
            { "gpt-3.5-turbo-0125", (Gpt35TurboInputCostPerMillion, Gpt35TurboOutputCostPerMillion) },
            { "gpt-3.5-turbo-instruct", (Gpt35TurboInstructInputCostPerMillion, Gpt35TurboInstructOutputCostPerMillion) }
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
