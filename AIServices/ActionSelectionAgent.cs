using DBTransferProject.Models;
using DBTransferProject.Services;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace DBTransferProject.AIServices
{
    public class ActionSelectionAgent
    {
        private readonly ILogger<ActionSelectionAgent> _logger;
        private readonly CarrierTrackingAgent _carrierTrackingAgent;
        private readonly MockJDAService _mockJDAService;

        public ActionSelectionAgent(ILogger<ActionSelectionAgent> logger, CarrierTrackingAgent carrierTrackingAgent, MockJDAService mockJDAService)
        {
            _logger = logger;
            _carrierTrackingAgent = carrierTrackingAgent;
            _mockJDAService = mockJDAService;
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
            if (aiResponse.Keywords.TrackingNumber.Count > 0 && !aiResponse.Keywords.TrackingNumber.Contains("N/A"))
            {
                // Split the tracking numbers into an array
                var trackingNumbers = aiResponse.Keywords.TrackingNumber;

                // Process each tracking number separately
                foreach (var trackingNumber in trackingNumbers)
                {
                    var trackingResult = await _carrierTrackingAgent.ProcessAsync(trackingNumber);

                    var carrier = _carrierTrackingAgent.DetermineCarrier(trackingNumber);
                    TrackingResult trackingConResult = JsonConvert.DeserializeObject<TrackingResult>(trackingResult);

                    aiResponse.TrackingResults.Add(new TrackingInformation
                    {
                        TrackingNumber = trackingNumber,
                        Carrier = carrier,
                        TrackingResult = trackingConResult

                    });
                }
            }
            // Check for and process order numbers
            else if (aiResponse.Keywords.OrderNumber != null && aiResponse.Keywords.OrderNumber.Count > 0)
            {
                
                foreach (var orderNumber in aiResponse.Keywords.OrderNumber)
                {
                    OrderInfo orderInfo = _mockJDAService.GetOrderInfo(orderNumber);
                    string trackingNumber = orderInfo.TrackingNumber;

                    if (orderInfo != null && !string.IsNullOrEmpty(trackingNumber))
                    {
                        var trackingResult = await _carrierTrackingAgent.ProcessAsync(trackingNumber);
                        var carrier = _carrierTrackingAgent.DetermineCarrier(trackingNumber);
                        TrackingResult trackingConResult = JsonConvert.DeserializeObject<TrackingResult>(trackingResult);

                        aiResponse.TrackingResults.Add(new TrackingInformation
                        {
                            TrackingNumber =  trackingNumber,
                            Carrier = carrier,
                            TrackingResult = trackingConResult
                            
                        });


                    }
                    else
                    {
                        _logger.LogWarning("Order number {OrderNumber} found but no tracking number available in the JDA order data.", orderNumber);
                    }
                }
            }
            else
            {
                _logger.LogWarning("Tracking number or order number not found for email category: {Category}", aiResponse.Category);
            }

            return aiResponse;
        }

    }
}
