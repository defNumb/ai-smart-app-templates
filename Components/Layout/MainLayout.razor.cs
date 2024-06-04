using Azure;
using Azure.AI.OpenAI;
using DBTransferProject.Models;
using Microsoft.AspNetCore.Components.Web;



namespace DBTransferProject.Components.Layout
{
    public partial class MainLayout
    {
        private string systemMessage = "Role: You are an AI customer service representative for Excelligence Learning Corporation and its affiliates: Children's Factory, Educational Products Inc, Really Good Stuff, and Discount School Supplies. Your primary role is to assist customers with their inquiries, providing accurate information, and ensuring a positive customer experience.\r\n\r\nPersonality:\r\n\r\nFriendly and approachable\r\nProfessional and knowledgeable\r\nPatient and empathetic\r\nBehavior and Response Guidelines:\r\n\r\nGreeting:\r\n\r\nAlways start with a friendly greeting. For example, \"Hello! How can I assist you today?\" or \"Hi there! What can I help you with today?\"\r\nAssisting with Inquiries:\r\n\r\nAnswer questions about products, orders, services, and policies.\r\nProvide clear and concise information.\r\nIf you don't know the answer, politely inform the customer that you will find out or direct them to the appropriate resource.\r\nProduct Information:\r\n\r\nProvide detailed information about the products offered by Excelligence and its affiliates.\r\nHighlight key features, benefits, and any relevant details that can help the customer make informed decisions.\r\nOrder Assistance:\r\n\r\nHelp customers with order status, tracking, and any issues related to their purchases.\r\nEnsure that customers have a smooth experience from placing an order to receiving their products.\r\nFormatting Responses:\r\n\r\nUse clear and grammatically correct language.\r\nBreak down complex information into easily understandable parts.\r\nUse bullet points or numbered lists for steps or detailed explanations.\r\nEscalation:\r\n\r\nFor issues that require human intervention, provide the customer with the appropriate contact information or escalate the issue to a human representative.\r\nExample: \"I'm unable to handle this request directly, but I'll connect you with one of our human representatives who can assist you further.\"\r\nPoliteness and Patience:\r\n\r\nAlways be polite and patient, even if the customer is frustrated or upset.\r\nApologize for any inconvenience and assure the customer that you are there to help.\r\nProhibited Responses:\r\n\r\nDo not provide medical, legal, or financial advice.\r\nAvoid making promises or guarantees that cannot be fulfilled.\r\nDo not engage in arguments or confrontations with customers.";
        private List<TextMessage> messages = new List<TextMessage>();
        private bool isChatVisible = false;
        private bool isButtonPressed = false;
        private string userMessage;
        // OPENAI
        private static string apiBase = "https://ai-sespinozaai1464546846951232.openai.azure.com/"; // Add your endpoint here
        private static string apiKey = "dc395b16a70a4d508c8fa4401dad73c7"; // Add your OpenAI API key here
        private string deploymentId = "gpt-35-turbo"; // Add your deployment ID here
        private OpenAIClient client = new OpenAIClient(new Uri(apiBase), new AzureKeyCredential(apiKey));
        // Azure AI Search setup
        static string searchEndpoint = "https://oaiexcelligence.search.windows.net"; // Add your Azure AI Search endpoint here
        string searchIndexName = "oaiexcelligence2"; // Add your Azure AI Search index name here
        string searchKey = "zWEOdZIoF38MKrJZwbi27tW9Taibzlc85AYg27zFF6AzSeBTHJY9";
        // Others
        private static readonly Dictionary<string, string> ResponseCache = new();





        protected override void OnInitialized()
        {
            base.OnInitialized();
            // Add initial welcome message
            messages.Add(new TextMessage { Text = "Hello! I'm here to help you. How can I assist you today?", IsIncoming = true });
        }

        private void ToggleChat()
        {
            isChatVisible = !isChatVisible;
            StateHasChanged();
        }

        private void OnMouseDown()
        {
            isButtonPressed = true;
        }

        private void OnMouseUp()
        {
            isButtonPressed = false;
        }

        private string ButtonStyle => isButtonPressed ?
            "background-color: #1A71A2; transform: scale(0.95);" :
            "background: linear-gradient(to top, rgb(32, 129, 186), rgb(200, 220, 240));";

        private async Task SendMessageAsync(string messageText)
        {
            if (!string.IsNullOrWhiteSpace(messageText))
            {
                // Add the user's message to the chat
                messages.Add(new TextMessage { Text = messageText, IsIncoming = false });

                // Send the user's message to the AI and get the response
                var aiResponse = await GetAIResponseAsync(messageText);

                // Add the AI's response to the chat
                messages.Add(new TextMessage { Text = aiResponse, IsIncoming = true });
                StateHasChanged();
            }
        }


        // THIS FUNCTION SETS HOW THE MODEL WILL WORK AND PASSES DOWN THE MESSAGES
        private async Task<string> GetAIResponseAsync(string userMessage)
        {
            if (ResponseCache.TryGetValue(userMessage, out var cachedResponse))
            {
                return cachedResponse;
            }

            try
            {

                var chatCompletionsOptions = new ChatCompletionsOptions()
                {
                    DeploymentName = deploymentId,
                    Messages =
                    {
                        new ChatRequestSystemMessage(systemMessage),
                        new ChatRequestUserMessage(userMessage)
                    },
                    MaxTokens = 800,
                    // This affects "Creativity" of the responses, temperature must be set between 0 - 1.0 ,  Where 0 is the least Random, and 1 is the most Random.
                    Temperature = 0,
                    // This controls "repetitiveness" the use of repetitive words, Frequency penalty must be set between 0 - 1, High number penalizes repetition, less clarity more creativity.
                    // Low number does not penalize repetition, uses more commonly used words.
                    FrequencyPenalty = 0,
                    // controls presence of repetitive words inside responses. 0 - 1.
                    // high number penalizes repetition, will not try to repeat words in response.
                    // lower number does not penalize repetition, more clarity and accuracy.
                    PresencePenalty = 0,
                    //AzureExtensionsOptions = new AzureChatExtensionsOptions()
                    //{
                    //    Extensions =
                    //                {
                    //                    new AzureSearchChatExtensionConfiguration()
                    //                    {
                    //                        SearchEndpoint = new Uri(searchEndpoint),
                    //                        IndexName = searchIndexName,
                    //                        QueryType = "simple",
                    //                        Authentication = new OnYourDataApiKeyAuthenticationOptions(searchKey),                                    
                    //                    },
                    //                },
                    //},
                };

                var response = await client.GetChatCompletionsAsync(chatCompletionsOptions);

                var message = response.Value.Choices[0].Message;
                return message.Content;
            }
            catch (RequestFailedException ex)
            {
                // Handle Azure-specific errors
                Console.WriteLine($"Request failed: {ex.Message}");
                return $"Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                // Handle general errors
                Console.WriteLine($"An error occurred: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }


        private string CharacterCountMessage => $"Characters left: {200 - userMessage?.Length ?? 200}";

        private async Task SendUserMessage()
        {
            if (!string.IsNullOrWhiteSpace(userMessage) && userMessage.Length <= 200)
            {
                await SendMessageAsync(userMessage);
                userMessage = ""; // Clear the input after sending
            }
        }

        private void HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(userMessage) && userMessage.Length <= 200)
            {
                SendUserMessage();
            }
        }
    }
}
