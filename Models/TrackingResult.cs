namespace DBTransferProject.Models
{
    public class TrackingResult
    {
        public string TrackingNumber { get; set; }
        public string CarrierCode { get; set; }
        public string StatusByLocale { get; set; }
        public string Description { get; set; }
        public string ScanLocationCity { get; set; }
        public string ScanLocationState { get; set; }
        public string ScanLocationCountry { get; set; }
        public string ReasonDescription { get; set; }
        public string EstimatedDeliveryBegins { get; set; }
        public string EstimatedDeliveryEnds { get; set; }
    }
}
