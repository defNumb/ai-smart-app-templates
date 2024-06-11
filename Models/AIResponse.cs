namespace DBTransferProject.Models
{
    public class AIResponse
    {
        public string Category { get; set; }
        public string Sentiment { get; set; }
        public Keywords Keywords { get; set; }
        public ImportantInformation ImportantInformation { get; set; }
        public string Action { get; set; }
        public string EmailPrompt { get; set; }
        public string ErrorMessage { get; set; }
        public string RawResponse { get; set; }
        public double Cost { get; set; }
        public List<TrackingInformation> TrackingResults { get; set; } = new List<TrackingInformation>();
    }

    public class Keywords
    {
        public List<string> AccountNumber { get; set; } = new List<string>();
        public List<string> PoNumber { get; set; } = new List<string>();
        public List<string> ItemNumber { get; set; } = new List<string>();
        public List<string> OrderConfirmation { get; set; } = new List<string>();
        public List<string> OrderNumber { get; set; }
        public List<string> TrackingNumber { get; set; } = new List<string>();
        public List<string> Carrier { get; set; } = new List<string>();
    }

    public class ImportantInformation
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
    }

    public class TrackingInformation
    {
        public string TrackingNumber { get; set; }
        public string Carrier { get; set; }
        public TrackingResult TrackingResult { get; set; }
    }
}
