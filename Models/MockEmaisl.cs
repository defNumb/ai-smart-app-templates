using System;
using System.Collections.Generic;
using Azure;
using DBTransferProject.Components.Pages;
using DBTransferProject.Services;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.Graph.Models;
using Microsoft.VisualBasic;

namespace DBTransferProject.Models
{
    public static class MockEmails
    {
        public static List<ApiConversation> GetMockConversations()
        {
            return new List<ApiConversation>
            {
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Urgent: Order Inquiry",
                        Preview = "Hi ,\r\n\r\nI hope you are doing well today!\r\n\r\nI am reaching out to confirm the status of these Orders. Could you please\r\nprovide tracking information for this order? If no tracking is available,\r\ncould you please provide an ETA?\r\n\r\n W9106556- FedEx: 449044304137821\r\n\r\nW9079017- FedEx: 020207021381215\r\n\r\nW9088307-FedEx: 039813852990618",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-30),
                                Subject = "Urgent: Order Inquiry",
                                Preview = "Hi ,\r\n\r\nI hope you are doing well today!\r\n\r\nI am reaching out to confirm the status of these Orders. Could you please\r\nprovide tracking information for this order ? If no tracking is available,\r\ncould you please provide an ETA ?\r\n\r\n W9106556 - FedEx : 449044304137821\r\n\r\nW9079017 - FedEx: 020207021381215\r\n\r\nW9088307 - FedEx: 039813852990618",
                                Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
    }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Shipping Issue - Please Help!",
                        Preview = "Hello, I've been tracking my package with the tracking number 122816215025810, and it seems to be stuck in transit. It was supposed to be delivered two days ago. I'm getting really worried. Can you please investigate and let me know what's causing the delay? I would greatly appreciate your help.",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-60),
                                Subject = "Shipping Issue - Please Help!",
                                Preview = "Hello, I've been tracking my package with the tracking number 122816215025810, and it seems to be stuck in transit. It was supposed to be delivered two days ago. I'm getting really worried. Can you please investigate and let me know what's causing the delay? I would greatly appreciate your help.",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Missing Items in Order #67890",
                        Preview = "Dear customer service, I just received my order #67890, but unfortunately, some items are missing. I double-checked the packing slip, and it seems that you forgot to include two out of the five items I ordered. Can you please look into this and send me the missing items as soon as possible? Let me know if you need any further information from me.",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-90),
                                Subject = "Missing Items in Order #67890",
                                Preview = "Dear customer service, I just received my order #67890, but unfortunately, some items are missing. I double-checked the packing slip, and it seems that you forgot to include two out of the five items I ordered. Can you please look into this and send me the missing items as soon as possible? Let me know if you need any further information from me.",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Damaged Product Received",
                        Preview = "Hello, I am writing to inform you that the product I received from my recent order #24680 arrived damaged. The packaging was fine, but when I opened it, I noticed that the item itself was cracked and not functioning properly. I am extremely disappointed with the quality control. I would like to request a replacement or a full refund. Please advise me on the next steps. Attached are the photos of the damaged product.",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-120),
                                Subject = "Damaged Product Received",
                                Preview = "Hello, I am writing to inform you that the product I received from my recent order #24680 arrived damaged. The packaging was fine, but when I opened it, I noticed that the item itself was cracked and not functioning properly. I am extremely disappointed with the quality control. I would like to request a replacement or a full refund. Please advise me on the next steps. Attached are the photos of the damaged product.",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Return Request for Order #13579",
                        Preview = "Hi, I recently purchased a few items from your store, but unfortunately, they didn't meet my expectations. The sizes were not as described on your website, and the colors were slightly off. I would like to return these items for a refund. The order number is 13579. Can you please guide me through the return process? I would appreciate a prompt response.",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-150),
                                Subject = "Return Request for Order #13579",
                                Preview = "Hi, I recently purchased a few items from your store, but unfortunately, they didn't meet my expectations. The sizes were not as described on your website, and the colors were slightly off. I would like to return these items for a refund. The order number is 13579. Can you please guide me through the return process? I would appreciate a prompt response.",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Urgent: Order Cancellation Request",
                        Preview = "Dear customer support, I need to cancel my order #98765 immediately. I accidentally placed the order twice, and I only need one of the items. I tried to cancel it through my account, but it seems that the order has already been processed. Please cancel one of the orders and refund the charges. I apologize for any inconvenience caused.",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-180),
                                Subject = "Urgent: Order Cancellation Request",
                                Preview = "Dear customer support, I need to cancel my order #98765 immediately. I accidentally placed the order twice, and I only need one of the items. I tried to cancel it through my account, but it seems that the order has already been processed. Please cancel one of the orders and refund the charges. I apologize for any inconvenience caused.",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Order Status Inquiry - #54321",
                        Preview = "Hello, I placed an order with your company last week, and I haven't received any updates on its status. The order number is 54321. I'm a bit concerned because I haven't received a shipping confirmation yet. Could you please check on the status of my order and let me know when I can expect it to be shipped? I would really appreciate an update.",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-210),
                                Subject = "Order Status Inquiry - #54321",
                                Preview = "Hello, I placed an order with your company last week, and I haven't received any updates on its status. The order number is 54321. I'm a bit concerned because I haven't received a shipping confirmation yet. Could you please check on the status of my order and let me know when I can expect it to be shipped? I would really appreciate an update.",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Wrong Item Received - Order #11223",
                        Preview = "Hi there, I just received my order #11223, but it seems that you sent me the wrong item. I ordered a blue sweatshirt in size medium, but I received a green one in size large. I double-checked my order confirmation, and it clearly states the correct item and size. Can you please send me the correct item as soon as possible? I need it for a gift, and I'm running out of time.",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-240),
                                Subject = "Wrong Item Received - Order #11223",
                                Preview = "Hi there, I just received my order #11223, but it seems that you sent me the wrong item. I ordered a blue sweatshirt in size medium, but I received a green one in size large. I double-checked my order confirmation, and it clearly states the correct item and size. Can you please send me the correct item as soon as possible? I need it for a gift, and I'm running out of time.",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Billing Issue with Order #44556",
                        Preview = "Hello, I'm contacting you regarding a billing issue with my recent order #44556. I was charged twice for the same item, and I only received one. I checked my bank statement, and the duplicate charge is clearly visible. Can you please investigate this issue and refund the extra charge? I have attached a screenshot of my bank statement for your reference.",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-270),
                                Subject = "Billing Issue with Order #44556",
                                Preview = "Hello, I'm contacting you regarding a billing issue with my recent order #44556. I was charged twice for the same item, and I only received one. I checked my bank statement, and the duplicate charge is clearly visible. Can you please investigate this issue and refund the extra charge? I have attached a screenshot of my bank statement for your reference.",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Missing Order Confirmation - #77889",
                        Preview = "Dear customer service, I placed an order on your website a couple of days ago, but I haven't received an order confirmation email yet. The order number is 77889. I'm a bit worried that my order might not have gone through successfully. Can you please check if my order was processed and send me the confirmation email? I want to make sure everything is on track.",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-300),
                                Subject = "Missing Order Confirmation - #77889",
                                Preview = "Dear customer service, I placed an order on your website a couple of days ago, but I haven't received an order confirmation email yet. The order number is 77889. I'm a bit worried that my order might not have gone through successfully. Can you please check if my order was processed and send me the confirmation email? I want to make sure everything is on track.",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Late Delivery Complaint",
                        Preview = "Hello, I am writing to complain about the late delivery of my order with tracking number XYZ456. According to your website, the estimated delivery date was supposed to be three days ago, but I still haven't received my package. I have been tracking the order, and it seems to be stuck in the same location for the past two days. This is unacceptable. I need this order urgently for a project. Can you please expedite the delivery and provide me with an updated estimated arrival date?",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-330),
                                Subject = "Late Delivery Complaint",
                                Preview = "Hello, I am writing to complain about the late delivery of my order with tracking number XYZ456. According to your website, the estimated delivery date was supposed to be three days ago, but I still haven't received my package. I have been tracking the order, and it seems to be stuck in the same location for the past two days. This is unacceptable. I need this order urgently for a project. Can you please expedite the delivery and provide me with an updated estimated arrival date?",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Product Inquiry - SKU 99887",
                        Preview = "Hi there, I'm interested in purchasing the product with SKU 99887, but I have a few questions before placing my order. Can you please provide me with more details about the product's dimensions and weight? Also, is this product compatible with the accessories I already have? I want to make sure it will fit my needs before I make the purchase. Any additional information you can provide would be greatly appreciated.",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-360),
                                Subject = "Product Inquiry - SKU 99887",
                                Preview = "Hi there, I'm interested in purchasing the product with SKU 99887, but I have a few questions before placing my order. Can you please provide me with more details about the product's dimensions and weight? Also, is this product compatible with the accessories I already have? I want to make sure it will fit my needs before I make the purchase. Any additional information you can give will be appreciated.",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                 new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Order Modification Request - #56473",
                        Preview = "Dear customer support, I recently placed an order with the order number 56473, but I need to make a modification to it. I accidentally selected the wrong color for one of the items. I wanted the black version instead of the blue one. Is it possible to change the color before the order is shipped? If not, can you please cancel the blue item and add the black one to my order? Please let me know if there are any additional charges for this modification.",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-390),
                                Subject = "Order Modification Request - #56473",
                                Preview = "Dear customer support, I recently placed an order with the order number 56473, but I need to make a modification to it. I accidentally selected the wrong color for one of the items. I wanted the black version instead of the blue one. Is it possible to change the color before the order is shipped? If not, can you please cancel the blue item and add the black one to my order? Please let me know if there are any additional charges for this modification.",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Refund Request for Order #48251",
                        Preview = "Hello, I am requesting a refund for my order #48251. I received the items, but they are not as described on your website. The quality is much lower than what I expected based on the product images and description. I am very disappointed with my purchase. I have already initiated a return request and sent the items back to you. Please process my refund as soon as you receive the returned package. Let me know if you need any further information from me.",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-420),
                                Subject = "Refund Request for Order #48251",
                                Preview = "Hello, I am requesting a refund for my order #48251. I received the items, but they are not as described on your website. The quality is much lower than what I expected based on the product images and description. I am very disappointed with my purchase. I have already initiated a return request and sent the items back to you. Please process my refund as soon as you receive the returned package. Let me know if you need any further information from me.",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Exchange Request for Order #75162",
                        Preview = "Hi there, I received my order #75162, but unfortunately, the size I ordered doesn't fit me properly. I would like to exchange it for a different size. I ordered a medium, but I think a large would be better suited for me. Can you please guide me through the exchange process? I want to make sure I get the right size this time. Also, will I be responsible for the shipping costs of the exchange? Please let me know. Thank you!",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-450),
                                Subject = "Exchange Request for Order #75162",
                                Preview = "Hi there, I received my order #75162, but unfortunately, the size I ordered doesn't fit me properly. I would like to exchange it for a different size. I ordered a medium, but I think a large would be better suited for me. Can you please guide me through the exchange process? I want to make sure I get the right size this time. Also, will I be responsible for the shipping costs of the exchange? Please let me know. Thank you!",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Where is my order?",
                        Preview = "Hello, I placed an order with your company, but I haven't received it yet. I don't have any information about the order status or tracking. Can you please help me locate my order? I'm getting really worried and frustrated as I need the items urgently. Please provide me with an update as soon as possible.",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-480),
                                Subject = "Where is my order?",
                                Preview = "Hello, I placed an order with your company, but I haven't received it yet. I don't have any information about the order status or tracking. Can you please help me locate my order? I'm getting really worried and frustrated as I need the items urgently. Please provide me with an update as soon as possible.",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Product Availability - SKU 78901",
                        Preview = "Dear customer service, I'm planning to place an order for the product with SKU 78901, but I couldn't find any information about its availability on your website. Can you please check if this product is currently in stock? If not, do you have an estimated restocking date? I want to make sure I can get it before the end of the month. Also, are there any ongoing promotions or discounts that I can apply to my order? Any information you can provide would be highly appreciated.",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-510),
                                Subject = "Product Availability - SKU 78901",
                                Preview = "Dear customer service, I'm planning to place an order for the product with SKU 78901, but I couldn't find any information about its availability on your website. Can you please check if this product is currently in stock? If not, do you have an estimated restocking date? I want to make sure I can get it before the end of the month. Also, are there any ongoing promotions or discounts that I can apply to my order? Any information you can provide would be highly appreciated.",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Technical Issue on Website",
                        Preview = "Hello, I'm trying to place an order on your website, but I'm experiencing some technical issues. Every time I try to add an item to my cart, I get an error message saying \"Something went wrong. Please try again later.\" I've tried clearing my browser cache and using a different browser, but the issue persists. This is very frustrating as I've been trying to place this order for the past hour. Can you please investigate this issue and let me know how I can proceed with my purchase?",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-540),
                                Subject = "Technical Issue on Website",
                                Preview = "Hello, I'm trying to place an order on your website, but I'm experiencing some technical issues. Every time I try to add an item to my cart, I get an error message saying \"Something went wrong. Please try again later.\" I've tried clearing my browser cache and using a different browser, but the issue persists. This is very frustrating as I've been trying to place this order for the past hour. Can you please investigate this issue and let me know how I can proceed with my purchase?",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Feedback on Recent Purchase",
                        Preview = "Dear team, I recently purchased a few items from your store, and I wanted to share my feedback. Overall, I'm satisfied with the products I received. The quality is good, and they match the descriptions on your website. However, I do have a suggestion for improvement. I think it would be helpful if you could provide more detailed sizing information for each product. I had to return one item because it didn't fit as expected. Other than that, I'm happy with my purchase, and I appreciate the prompt shipping. Keep up the good work!",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-570),
                                Subject = "Feedback on Recent Purchase",
                                Preview = "Dear team, I recently purchased a few items from your store, and I wanted to share my feedback. Overall, I'm satisfied with the products I received. The quality is good, and they match the descriptions on your website. However, I do have a suggestion for improvement. I think it would be helpful if you could provide more detailed sizing information for each product. I had to return one item because it didn't fit as expected. Other than that, I'm happy with my purchase, and I appreciate the prompt shipping. Keep up the good work!",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                },
                new ApiConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Attributes = new ApiConversationAttributes
                    {
                        Name = "Issue with Order #32415",
                        Preview = "Hello, I received my order #32415 yesterday, but there seems to be an issue with one of the items. The packaging was damaged, and when I opened it, I noticed that the product had a few scratches on it. It looks like it was mishandled during shipping. I'm disappointed with the condition of the item, especially considering the price I paid for it. Can you please let me know what my options are? I would like to get a replacement or a partial refund. Attached are the photos of the damaged item for your reference.",
                        Status = "open"
                    },
                    Messages = new List<ApiMessage>
                    {
                        new ApiMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Attributes = new ApiMessagesAttributes
                            {
                                SentAt = DateTime.Now.AddMinutes(-600),
                                Subject = "Issue with Order #32415",
                                Preview = "Hello, I received my order #32415 yesterday, but there seems to be an issue with one of the items. The packaging was damaged, and when I opened it, I noticed that the product had a few scratches on it. It looks like it was mishandled during shipping. I'm disappointed with the condition of the item, especially considering the price I paid for it. Can you please let me know what my options are? I would like to get a replacement or a partial refund. Attached are the photos of the damaged item for your reference.",
                            Meta = new ApiMessagesMeta
                                {
                                    From = "customer_email@mail.com"
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}