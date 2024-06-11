using DBTransferProject.Models;

namespace DBTransferProject.Services
{
    public class MockJDAService
    {
        private readonly List<OrderInfo> _orders;

        public MockJDAService()
        {
            _orders = new List<OrderInfo>
        {
            new OrderInfo { OrderNumber = "P3989567", CustomerName = "John Doe", Address = "123 Elm St, Springfield, IL", Institution = "Springfield High School", TrackingNumber = "20207021381215", CustomerAccountNumber = "005264395", OrderDate = DateTime.Now.AddDays(-10), EstimatedDeliveryDate = DateTime.Now.AddDays(5) },
            new OrderInfo { OrderNumber = "W3923467", CustomerName = "Jane Smith", Address = "456 Oak St, Springfield, IL", Institution = "Springfield Elementary", TrackingNumber = "149331877648230", CustomerAccountNumber = "005222690", OrderDate = DateTime.Now.AddDays(-8), EstimatedDeliveryDate = DateTime.Now.AddDays(3) },
            new OrderInfo { OrderNumber = "P4275941", CustomerName = "Alice Johnson", Address = "789 Pine St, Springfield, IL", Institution = "Springfield Middle School", TrackingNumber = "039813852990618", CustomerAccountNumber = "5939996", OrderDate = DateTime.Now.AddDays(-15), EstimatedDeliveryDate = DateTime.Now.AddDays(7) },
            new OrderInfo { OrderNumber = "W1424365", CustomerName = "Bob Brown", Address = "321 Maple St, Springfield, IL", Institution = "Springfield Elementary", TrackingNumber = "957794015041323", CustomerAccountNumber = "16076150", OrderDate = DateTime.Now.AddDays(-5), EstimatedDeliveryDate = DateTime.Now.AddDays(10) },
            new OrderInfo { OrderNumber = "P4277836", CustomerName = "Carol White", Address = "654 Birch St, Springfield, IL", Institution = "Springfield High School", TrackingNumber = "843119172384577", CustomerAccountNumber = "53207460", OrderDate = DateTime.Now.AddDays(-12), EstimatedDeliveryDate = DateTime.Now.AddDays(4) },
            new OrderInfo { OrderNumber = "P4277885", CustomerName = "David Green", Address = "987 Cedar St, Springfield, IL", Institution = "Springfield Middle School", TrackingNumber = "076288115212522", CustomerAccountNumber = "34384693", OrderDate = DateTime.Now.AddDays(-20), EstimatedDeliveryDate = DateTime.Now.AddDays(6) },
            new OrderInfo { OrderNumber = "P4136521", CustomerName = "Eve Black", Address = "159 Willow St, Springfield, IL", Institution = "Springfield Elementary", TrackingNumber = "797806677146", CustomerAccountNumber = "52942711", OrderDate = DateTime.Now.AddDays(-7), EstimatedDeliveryDate = DateTime.Now.AddDays(2) },
            new OrderInfo { OrderNumber = "W1834657", CustomerName = "Frank Blue", Address = "852 Ash St, Springfield, IL", Institution = "Springfield High School", TrackingNumber = "231300687629630", CustomerAccountNumber = "005264395", OrderDate = DateTime.Now.AddDays(-14), EstimatedDeliveryDate = DateTime.Now.AddDays(3) },
            new OrderInfo { OrderNumber = "P2547865", CustomerName = "Grace Yellow", Address = "753 Chestnut St, Springfield, IL", Institution = "Springfield Middle School", TrackingNumber = "403934084723025", CustomerAccountNumber = "005222690", OrderDate = DateTime.Now.AddDays(-10), EstimatedDeliveryDate = DateTime.Now.AddDays(5) },
            new OrderInfo { OrderNumber = "W3987564", CustomerName = "Hank Gray", Address = "963 Cypress St, Springfield, IL", Institution = "Springfield Elementary", TrackingNumber = "020207021381215", CustomerAccountNumber = "5939996", OrderDate = DateTime.Now.AddDays(-9), EstimatedDeliveryDate = DateTime.Now.AddDays(4) }
        };
        }

        public OrderInfo GetOrderInfo(string orderNumber)
        {
            return _orders.FirstOrDefault(o => o.OrderNumber.Equals(orderNumber, StringComparison.OrdinalIgnoreCase));
        }
    }
}
