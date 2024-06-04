using System;
using System.Net.Http;
using System.Threading.Tasks;
using DBTransferProject.AIServices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net.Http.Headers;
using DBTransferProject.Models;
namespace DBTransferProject.AIServices
{
    public class CarrierTrackingAgent : IAIServiceAgent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CarrierTrackingAgent> _logger;

        public CarrierTrackingAgent(IHttpClientFactory httpClientFactory, ILogger<CarrierTrackingAgent> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<string> ProcessAsync(string input)
        {
            try
            {
                var trackingInfo = JObject.Parse(input);
                var carrier = trackingInfo["Carrier"]?.ToString();
                var trackingNumber = trackingInfo["TrackingNumber"]?.ToString();

                if (string.IsNullOrEmpty(carrier) || string.IsNullOrEmpty(trackingNumber))
                {
                    _logger.LogWarning("Carrier or tracking number is missing in the input: {input}", input);
                    return string.Empty;
                }

                switch (carrier.ToLower())
                {
                    case "fedex":
                        var fedExTrackingResult = await GetFedExTrackingInfoAsync(trackingNumber);
                        return JsonConvert.SerializeObject(fedExTrackingResult);

                    case "ups":
                        var upsTrackingResult = await GetUPSTrackingInfoAsync(trackingNumber);
                        return JsonConvert.SerializeObject(upsTrackingResult);

                    case "usps":
                        var uspsTrackingResult = await GetUSPSTrackingInfoAsync(trackingNumber);
                        return JsonConvert.SerializeObject(uspsTrackingResult);

                    // Add more carriers as needed

                    default:
                        _logger.LogWarning("Unsupported carrier: {carrier}", carrier);
                        return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing tracking information: {input}", input);
                throw;
            }
        }


        // ************************************************************************************
        //**************************** FEDEX TRACKING *****************************************
        public async Task<TrackingResult> GetFedExTrackingInfoAsync(string trackingNumber)
        {
            var client = _httpClientFactory.CreateClient();

            // Step 1: Obtain JWT
            var token = await GetFedExJwtAsync(client);

            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("Failed to obtain FedEx JWT.");
            }

            // Step 2: Set up the request to the tracking API
            var requestUrl = "https://apis-sandbox.fedex.com/track/v1/trackingnumbers"; // FedEx API endpoint

            var requestBody = new
            {
                includeDetailedScans = true,
                trackingInfo = new[]
                {
            new
            {
                trackingNumberInfo = new
                {
                    trackingNumber
                }
            }
        }
            };

            var requestJson = JsonConvert.SerializeObject(requestBody);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            requestMessage.Headers.Add("X-locale", "en_US");
            requestMessage.Headers.Add("X-customer-transaction-id", Guid.NewGuid().ToString());

            var response = await client.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var trackResult = JObject.Parse(responseContent);
            var completeTrackResults = trackResult["output"]?["completeTrackResults"]?.FirstOrDefault();
            var trackingNumberInfo = completeTrackResults?["trackResults"]?[0]?["trackingNumberInfo"];
            var latestStatusDetail = completeTrackResults?["trackResults"]?[0]?["latestStatusDetail"];
            var ancillaryDetails = latestStatusDetail?["ancillaryDetails"]?[0];
            var estimatedDeliveryTimeWindow = completeTrackResults?["trackResults"]?[0]?["estimatedDeliveryTimeWindow"]?["window"];

            var trackingResult = new TrackingResult
            {
                TrackingNumber = trackingNumberInfo?["trackingNumber"]?.ToString(),
                CarrierCode = trackingNumberInfo?["carrierCode"]?.ToString(),
                StatusByLocale = latestStatusDetail?["statusByLocale"]?.ToString(),
                Description = latestStatusDetail?["description"]?.ToString(),
                ScanLocationCity = latestStatusDetail?["scanLocation"]?["city"]?.ToString(),
                ScanLocationState = latestStatusDetail?["scanLocation"]?["stateOrProvinceCode"]?.ToString(),
                ScanLocationCountry = latestStatusDetail?["scanLocation"]?["countryCode"]?.ToString(),
                ReasonDescription = ancillaryDetails?["reasonDescription"]?.ToString(),
                EstimatedDeliveryBegins = estimatedDeliveryTimeWindow?["begins"]?.ToString(),
                EstimatedDeliveryEnds = estimatedDeliveryTimeWindow?["ends"]?.ToString()
            };

            return trackingResult;
        }

        private async Task<string> GetFedExJwtAsync(HttpClient client)
        {
            var tokenUrl = "https://apis-sandbox.fedex.com/oauth/token";

            var requestBody = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("grant_type", "client_credentials"),
        new KeyValuePair<string, string>("client_id", "l7061b36d0f4d94b1cb7324a6a45a33e16"), // Replace with your client_id
        new KeyValuePair<string, string>("client_secret", "f3c1593159d346fb8f20424f7b4c9f60") // Replace with your client_secret
    });

            requestBody.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await client.PostAsync(tokenUrl, requestBody);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

            return tokenResponse?.AccessToken;
        }

        private class TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("scope")]
            public string Scope { get; set; }
        }
        // ************************************************************************************
        //**************************** UPS TRACKING *****************************************
        private async Task<string> GetUPSTrackingInfoAsync(string trackingNumber)
        {
            var client = _httpClientFactory.CreateClient();
            var requestUrl = $"https://api.ups.com/tracking/{trackingNumber}"; // Replace with actual UPS API endpoint

            var response = await client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            // Parse the response content and return the tracking info
            return responseContent;
        }

        private async Task<string> GetUSPSTrackingInfoAsync(string trackingNumber)
        {
            var client = _httpClientFactory.CreateClient();
            var requestUrl = $"https://api.usps.com/tracking/{trackingNumber}"; // Replace with actual USPS API endpoint

            var response = await client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            // Parse the response content and return the tracking info
            return responseContent;
        }
    }
}