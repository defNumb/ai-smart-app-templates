using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using DBTransferProject.Hubs;
using DBTransferProject.Models;
using DBTransferProject.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
namespace DBTransferProject.Controllers
{
    [Route("api/webhook/kustomer")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    public class KustomerWebhookController : ControllerBase
    {
        private readonly IHubContext<KustomerHub> _hubContext;
        private readonly KustomerService _kustomerService;
        private readonly ILogger<KustomerWebhookController> _logger;

        public KustomerWebhookController(IHubContext<KustomerHub> hubContext, KustomerService kustomerService, ILogger<KustomerWebhookController> logger)
        {
            _hubContext = hubContext;
            _kustomerService = kustomerService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            //_logger.LogInformation("Webhook received");

            string jsonString;
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    jsonString = await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading request body");
                return StatusCode(500, "Error reading request body");
            }

            KustomerWebhookPayload payload;


            try
            {
                payload = JsonSerializer.Deserialize<KustomerWebhookPayload>(jsonString);
                if (payload == null)
                {
                    _logger.LogWarning("Deserialized payload is null");
                    return BadRequest("Payload is null");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing webhook payload: {Payload}", jsonString);
                return BadRequest("Invalid payload");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deserializing webhook payload: {Payload}", jsonString);
                return StatusCode(500, "Internal server error");
            }


            try
            {
                if (payload.Data.Type == "conversation")
                {
                    try
                    {
                        var conversation = await _kustomerService.GetConversationByIdAsync(payload.Data.Id);
                        await _hubContext.Clients.All.SendAsync("ReceiveEvent", conversation);
                        if (conversation != null)
                        {
                            await _hubContext.Clients.All.SendAsync("ReceiveConversation", conversation);
                        }
                        else
                        {
                            _logger.LogWarning("Conversation not found: {Id}", payload.Data.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing conversation event: {Payload}", JsonSerializer.Serialize(payload));
                    }
                }
                //else if (payload.Data.Type == "message")
                //{
                //    try
                //    {

                //        _logger.LogInformation("New message event detected: {Id}", payload.Data.Id);
                   
                //            await _hubContext.Clients.All.SendAsync("ReceiveMessage", payload);
                //            _logger.LogInformation("Message sent to clients: {Id}", payload.Data.Id);

                //    }
                //    catch (Exception ex)
                //    {
                //        _logger.LogError(ex, "Error processing message event: {Payload}", JsonSerializer.Serialize(payload));
                //    }
                //}
                //else
                //{
                //    _logger.LogWarning("Unhandled event type: {Type}", payload.Data.Type);
                //}

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook payload: {Payload}", JsonSerializer.Serialize(payload));
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class KustomerWebhookPayload
    {

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("org")]
        public string Org { get; set; }

        [JsonPropertyName("partition")]
        public string Partition { get; set; }

        [JsonPropertyName("data")]
        public KustomerWebhookData Data { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("persist")]
        public bool Persist { get; set; }

        [JsonPropertyName("client")]
        public string Client { get; set; }

        [JsonPropertyName("sourceId")]
        public string SourceId { get; set; }

        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; }

        [JsonPropertyName("dataId")]
        public string DataId { get; set; }
    }

    public class KustomerWebhookData
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("attributes")]
        public KustomerWebhookAttributes? Attributes { get; set; }

        [JsonPropertyName("relationships")]
        public KustomerWebhookRelationships? Relationships { get; set; }
    }

    public class KustomerWebhookMeta
    {
        [JsonPropertyName("from")]
        public string? From { get; set; }
    }

    public class KustomerWebhookAttributes
    {
        [JsonPropertyName("preview")]
        public string? Preview { get; set; }

        [JsonPropertyName("subject")]
        public string? Subject { get; set; }


        [JsonPropertyName("meta")]
        public KustomerWebhookMeta? Meta { get; set; }

        [JsonPropertyName("sentAt")]
        public string? SentAt { get; set; }

    }
    public class KustomerWebhookRelationships
    {
        [JsonPropertyName("conversation")]
        public KustomerWebhookConversation? Conversation { get; set; }

    }
    public class KustomerWebhookConversation
    {
        [JsonPropertyName("data")]
        public KustomerWebhookConversationData? Data { get; set; }

    }
    public class KustomerWebhookConversationData
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

    }
}