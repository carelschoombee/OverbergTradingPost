using System.ComponentModel.DataAnnotations;

namespace FreeMarket.Models
{
    [MetadataType(typeof(CashOrderDetailMetaData))]
    public partial class CashOrderDetail
    {
        public string Description { get; set; }
        public string SupplierName { get; set; }
        public int Weight { get; set; }
    }

    public class CashOrderDetailMetaData
    {

    }
}