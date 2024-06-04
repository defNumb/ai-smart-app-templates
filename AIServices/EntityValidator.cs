using System;
using System.Linq;
using System.Threading.Tasks;
using DBTransferProject.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DBTransferProject.AIServices
{
    public class EntityValidator
    {
        public bool ValidateKeywords(string originalTextMessage, AIResponse aiResponse)
        {
            // Check AccountNumber
            if (!string.IsNullOrEmpty(aiResponse.Keywords.AccountNumber) &&
                !originalTextMessage.Contains(aiResponse.Keywords.AccountNumber))
            {
                return false;
            }

            // Check PoNumber
            if (!string.IsNullOrEmpty(aiResponse.Keywords.PoNumber) &&
                !originalTextMessage.Contains(aiResponse.Keywords.PoNumber))
            {
                return false;
            }

            // Check ItemNumber
            foreach (var itemNumber in aiResponse.Keywords.ItemNumber)
            {
                if (!string.IsNullOrEmpty(itemNumber) && !originalTextMessage.Contains(itemNumber))
                {
                    return false;
                }
            }

            // Check OrderConfirmation
            if (!string.IsNullOrEmpty(aiResponse.Keywords.OrderConfirmation) &&
                !originalTextMessage.Contains(aiResponse.Keywords.OrderConfirmation))
            {
                return false;
            }

            // Check OrderNumber
            foreach (var orderNumber in aiResponse.Keywords.OrderNumber)
            {
                if (!string.IsNullOrEmpty(orderNumber) && !originalTextMessage.Contains(orderNumber))
                {
                    return false;
                }
            }

            // Check TrackingNumber
            if (!string.IsNullOrEmpty(aiResponse.Keywords.TrackingNumber) &&
                !originalTextMessage.Contains(aiResponse.Keywords.TrackingNumber))
            {
                return false;
            }

            return true;
        }
    }
}