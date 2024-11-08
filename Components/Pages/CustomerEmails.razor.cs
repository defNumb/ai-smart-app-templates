﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using DBTransferProject.Models;
using DBTransferProject.Services;
using DBTransferProject.AIServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using DBTransferProject.Controllers;
using Newtonsoft.Json;

namespace DBTransferProject.Components.Pages
{
    public partial class CustomerEmails
    {
        private List<ApiConversation> conversations = new List<ApiConversation>();
        private List<ApiMessage> selectedMessages = new List<ApiMessage>();

        private string errorMessage;
        private HubConnection hubConnection;
        private string selectedConversationId;
        private string eventMessage;
        private AIResponse aiResponse;
        private bool isAnalyzing = false;
        private bool useMockEmails = false;

        [Inject]
        private KustomerService KustomerService { get; set; }

        [Inject]
        private ILogger<CustomerEmails> Logger { get; set; }

        [Inject]
        private AgentOrchestrator AgentOrchestrator { get; set; }

        private string emailSourceButtonText => useMockEmails ? "Switch to Real Emails" : "Switch to Mock Emails";

        protected override async Task OnInitializedAsync()
        {
            try
            {
                hubConnection = new HubConnectionBuilder()
                    .WithUrl(Navigation.ToAbsoluteUri("/kustomerHub"))
                    .Build();

                hubConnection.On<string>("ReceiveEvent", (eventName) =>
                {
                    eventMessage = $"Received event: {eventName}";
                    InvokeAsync(StateHasChanged);
                });

                hubConnection.On<ApiConversation>("ReceiveConversation", async (conversation) =>
                {
                    try
                    {
                        if (conversation.Id != null && !conversations.Any(c => c.Id == conversation.Id))
                        {
                            // Retrieve the messages of the incoming conversation using the conversation ID
                            var messages = await KustomerService.GetMessagesByConversationIdAsync(conversation.Id);

                            // Initialize the FilterAgent
                            var filterAgent = new FilterAgent();

                            // Check if the messages are from excluded addresses
                            var validMessages = messages.Where(m => !filterAgent.IsMessageFromExcludedAddress(m)).ToList();

                            // Proceed only if there are valid messages
                            if (validMessages.Any())
                            {
                                foreach (var message in validMessages)
                                {
                                    Logger.LogInformation("Before Cleanup: {emailBody}", message.Attributes.Preview);
                                }

                                // Clean the valid messages
                                validMessages = filterAgent.FilterAndCleanMessages(validMessages);

                                foreach (var message in validMessages)
                                {
                                    Logger.LogInformation("After Cleanup: {emailBody}", message.Attributes.Preview);
                                }

                                // Create email content including subject and body
                                var emailContent = validMessages
                                    .Select(m => $"{m.Attributes.Meta.Subject} {m.Attributes.Preview}")
                                    .Aggregate((current, next) => current + " " + next);
                                var categoryResult = await AgentOrchestrator.GetCategoryResultAsync(emailContent);
                                var category = JObject.Parse(categoryResult)?["Category"]?.ToString() ?? string.Empty;
                                Logger.LogInformation("*******Category Result FRONTEND: {categoryResult}**********", category);

                                // If the returning category is "Tracking Information", "Order Inquiry", or "Shipping and Delivery", further analyze for tracking requests
                                if (category == "Tracking Information" || category == "Order Inquiry" || category == "Shipping and Delivery")
                                {
                                    var (isTrackingRequest, confidenceLevel) = await AgentOrchestrator.CheckForTrackingRequestAsync(emailContent);

                                    // Set a confidence threshold (e.g., 85%) for determining if the conversation should be added
                                    const double confidenceThreshold = 85.0;

                                    if (isTrackingRequest && confidenceLevel >= confidenceThreshold)
                                    {
                                        conversations.Insert(0,conversation);
                                        await InvokeAsync(StateHasChanged);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error handling received conversation");
                        errorMessage = $"Error handling received conversation: {ex.Message}";
                    }
                });


                await hubConnection.StartAsync();
                Logger.LogInformation("SignalR connection started");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error starting SignalR connection");
                errorMessage = $"Error starting SignalR connection: {ex.Message}";
            }
        }

        private async Task LoadMessages(string conversationId)
        {
            try
            {
                selectedMessages = await KustomerService.GetMessagesByConversationIdAsync(conversationId);
                Logger.LogInformation("Messages loaded for conversation: {Id}, Count: {Count}", conversationId, selectedMessages.Count);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading messages");
                errorMessage = $"Error loading messages: {ex.Message}";
            }
        }

        private async Task SelectConversation(string conversationId)
        {
            selectedConversationId = conversationId;
            if (useMockEmails)
            {
                var mockConversation = conversations.FirstOrDefault(c => c.Id == conversationId);
                if (mockConversation != null)
                {
                    selectedMessages = mockConversation.Messages;
                }
            }
            else
            {
                await LoadMessages(conversationId);
            }

            if (selectedMessages != null && selectedMessages.Any())
            {
                var filterAgent = new FilterAgent();

                selectedMessages = filterAgent.FilterAndCleanMessages(selectedMessages);

                var latestMessage = selectedMessages.OrderByDescending(m => m.Attributes.SentAt).FirstOrDefault();
                if (latestMessage != null)
                {
                    await AnalyzeMessage(latestMessage.Attributes.Meta.Subject + latestMessage.Attributes.Preview);
                }
            }

            await InvokeAsync(StateHasChanged);
        }

        private async Task AnalyzeMessage(string messageContent)
        {
            try
            {
                isAnalyzing = true;
                aiResponse = null;
                await InvokeAsync(StateHasChanged);

                var analysisResult = await AgentOrchestrator.ProcessEmailAsync(messageContent);
                aiResponse = JsonConvert.DeserializeObject<AIResponse>(analysisResult);

                isAnalyzing = false;
                await InvokeAsync(StateHasChanged);
            }
            catch (JsonReaderException ex)
            {
                Logger.LogError(ex, "Error parsing AI response JSON in AnalyzeMessage");
                errorMessage = $"Error during AI analysis: {ex.Message}";
                isAnalyzing = false;
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during AI analysis");
                errorMessage = $"Error during AI analysis: {ex.Message}";
                isAnalyzing = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        private void ToggleEmailSource()
        {
            useMockEmails = !useMockEmails;
            if (useMockEmails)
            {
                LoadMockEmails();
            }
            else
            {
                LoadRealEmails();
            }
        }

        private void LoadMockEmails()
        {
            conversations = MockEmails.GetMockConversations();
            selectedMessages = new List<ApiMessage>();
            aiResponse = null;
            StateHasChanged();
        }

        private async Task LoadRealEmails()
        {
            await LoadConversations();
            selectedMessages = new List<ApiMessage>();
            aiResponse = null;
            StateHasChanged();
        }

        private async Task LoadConversations()
        {
            try
            {
                conversations = await KustomerService.GetConversationsAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading conversations");
                errorMessage = $"Error loading conversations: {ex.Message}";
            }
        }

        private void ApproveAndSendEmail()
        {
            // Your logic to approve and send the email
        }

        private void DelegateToCustomerRep()
        {
            // Your logic to delegate to a human representative
        }
    }
}
