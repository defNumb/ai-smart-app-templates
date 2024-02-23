namespace DBTransferProject.Models
{
    public class DatabaseEntity
    {
        public int Id { get; set; } // Auto-incremented by the database
        public string? Key { get; set; } // Up to 10 characters
        public string? Name { get; set; } // Up to 100 characters
        public string? ProviderCredential { get; set; } // Up to 100 characters
        public string? UserIdentity { get; set; } // Up to 100 characters
        public string? AccountNumber { get; set; } // Should be exactly 10 characters, padded if necessary
        public string? KeyCode { get; set; } // Should be exactly 8 characters, padded if necessary
        public string? DiscountCode { get; set; } // Should be exactly 1 character
        public string? CustomerType { get; set; } // Should be exactly 2 characters, padded if necessary
        public string? HoldCode { get; set; } // Should be exactly 1 character
        public string? PurchaseOrderProcessor { get; set; } // Up to 100 characters
        public string? InvoiceProcessor { get; set; } // Up to 100 characters
        public string? InvoiceURL { get; set; } // Up to 200 characters
        public string? ShippingAcknowledgementProcessor { get; set; } // Up to 100 characters
        public string? ShippingAcknowledgementURL { get; set; } // Up to 200 characters
        public string? OrderAcknowledgementProcessor { get; set; } // Up to 100 characters
        public string? OrderAcknowledgementURL { get; set; } // Up to 200 characters
        public int? PrintPrices { get; set; }
        public int? OveridePrices { get; set; }
        public int? Expedite { get; set; }
        public int? UseSuppliedPH { get; set; }
        public int? UseAddressId { get; set; }
    }

}

