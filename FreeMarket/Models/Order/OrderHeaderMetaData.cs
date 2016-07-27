using System.ComponentModel.DataAnnotations;

namespace FreeMarket.Models
{
    [MetadataType(typeof(OrderHeaderMetaData))]
    public partial class OrderHeader
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPrimaryContactPhone { get; set; }
        public string CustomerPreferredCommunicationMethod { get; set; }

        public static OrderHeader CreateOrder() { return new OrderHeader(); }
    }
    public class OrderHeaderMetaData
    {
    }
}