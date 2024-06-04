namespace DBTransferProject.Models
{
    public class ImpexAddress
    {
        public string? Owner { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? PostalCode { get; set; }
        public string? Town { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public string? Phone1 { get; set; }
        public bool? BillingAddress { get; set; }
        public bool? ShippingAddress { get; set; }
    }
}
