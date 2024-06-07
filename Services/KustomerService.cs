using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DBTransferProject.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace DBTransferProject.Services
{
    public class KustomerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KustomerService> _logger;

        public KustomerService(HttpClient httpClient, ILogger<KustomerService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<ApiConversation>> GetConversationsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiConversationsResponse>("/v1/conversations?status=open");
                return response?.Data ?? new List<ApiConversation>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching conversations");
                throw;
            }
        }

        public async Task<List<ApiMessage>> GetMessagesByConversationIdAsync(string conversationId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiMessagesResponse>($"/v1/conversations/{conversationId}/messages");
                return response?.Data ?? new List<ApiMessage>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching messages for conversation {ConversationId}", conversationId);
                throw;
            }
        }

        public async Task<ApiConversation> GetConversationByIdAsync(string conversationId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiConversationResponse>($"/v1/conversations/{conversationId}");
                return response?.Data;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching conversation {ConversationId}", conversationId);
                throw;
            }
        }

        public async Task<ApiMessage> GetMessageByIdAsync(string messageId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiMessageResponse>($"/v1/messages/{messageId}");
                return response?.Data;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching message {MessageId}", messageId);
                throw;
            }
        }
    }

    public class ApiConversationsResponse
    {
        public List<ApiConversation> Data { get; set; }
    }

    public class ApiMessagesResponse
    {
        public List<ApiMessage> Data { get; set; }
    }

    public class ApiConversationResponse
    {
        public ApiConversation Data { get; set; }
    }

    public class ApiMessageResponse
    {
        public ApiMessage Data { get; set; }
    }

    public class ApiConversation
    {
        public string Id { get; set; }
        public ApiConversationAttributes Attributes { get; set; }
        public List<ApiMessage> Messages { get; set; }
    }

    public class ApiConversationAttributes
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Preview { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ApiMessage
    {
        public string Id { get; set; }
        public ApiMessagesAttributes Attributes { get; set; }
    }

    public class ApiMessagesAttributes
    {
        public DateTime SentAt { get; set; }
        public string Subject { get; set; }
        public string Status { get; set; }
        public string Preview { get; set; }
        public ApiMessagesMeta Meta { get; set; }
    }
    public class ApiMessagesMeta
    {
        public string From { get; set; }
        public string Subject { get; set; }  
    }
}
