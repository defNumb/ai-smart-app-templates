using DBTransferProject.Models;

namespace DBTransferProject.AIServices
{
    public class ActionSelectionAgent
    {
        private readonly ILogger<ActionSelectionAgent> _logger;
        private readonly CarrierTrackingAgent _carrierTrackingAgent;

        public ActionSelectionAgent(ILogger<ActionSelectionAgent> logger, CarrierTrackingAgent carrierTrackingAgent)
        {
            _logger = logger;
            _carrierTrackingAgent = carrierTrackingAgent;
        }

        public async Task<AIResponse> ProcessAsync(AIResponse aiResponse)
        {
            // Implement the logic to decide what to do with the customer email based on the category and extracted information
            switch (aiResponse.Category)
            {
                case "Order Inquiry":
                case "Shipping and Delivery":
                case "Tracking Information":
                    aiResponse = await HandleTrackingInformationAsync(aiResponse);
                    break;
                // Add more cases for other email categories and corresponding actions
                default:
                    _logger.LogWarning("No specific action defined for email category: {Category}", aiResponse.Category);
                    break;
            }

            return aiResponse;
        }

        private async Task<AIResponse> HandleTrackingInformationAsync(AIResponse aiResponse)
        {
            if (!string.IsNullOrEmpty(aiResponse.Keywords.TrackingNumber) && !string.IsNullOrEmpty(aiResponse.Keywords.Carrier) &&
                !aiResponse.Keywords.TrackingNumber.Contains("N/A"))
            {
                // Split the tracking numbers into an array
                var trackingNumbers = aiResponse.Keywords.TrackingNumber.Split(',').Select(x => x.Trim()).ToArray();

                // Process each tracking number separately
                foreach (var trackingNumber in trackingNumbers)
                {
                    var trackingInfo = new
                    {
                        Carrier = aiResponse.Keywords.Carrier,
                        TrackingNumber = trackingNumber
                    };

                    var trackingResult = await _carrierTrackingAgent.GetFedExTrackingInfoAsync(trackingNumber);

                    aiResponse.TrackingResults.Add(new TrackingInformation
                    {
                        TrackingNumber = trackingNumber,
                        Carrier = aiResponse.Keywords.Carrier,
                        TrackingResult = trackingResult
                    });
                }
            }
            else
            {
                _logger.LogWarning("Tracking number or carrier not found for email category: {Category}", aiResponse.Category);
            }

            return aiResponse;
        }
    }
}
