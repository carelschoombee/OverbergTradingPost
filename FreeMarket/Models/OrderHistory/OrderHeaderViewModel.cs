namespace FreeMarket.Models
{
    public class OrderHeaderViewModel
    {
        public string CustomerFullName { get; set; }
        public string DeliveryAddress { get; set; }
        public OrderHeader Order { get; set; }
    }
}