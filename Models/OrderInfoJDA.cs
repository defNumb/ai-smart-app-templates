namespace DBTransferProject.Models
{
    public class OrderInfo
    {
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string Institution { get; set; }
        public string TrackingNumber { get; set; }
        public string CustomerAccountNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
    }
}
