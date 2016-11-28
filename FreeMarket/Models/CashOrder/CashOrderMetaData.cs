using System.ComponentModel.DataAnnotations;

namespace FreeMarket.Models
{
    [MetadataType(typeof(CashOrderMetaData))]
    public partial class CashOrder
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerDeliveryAddress { get; set; }
    }

    public class CashOrderMetaData
    {

    }
}