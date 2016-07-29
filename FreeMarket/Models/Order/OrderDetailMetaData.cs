using System.ComponentModel.DataAnnotations;

namespace FreeMarket.Models
{
    [MetadataType(typeof(OrderDetailMetaData))]
    public partial class OrderDetail
    {
        public string SupplierName { get; set; }
        public string CourierName { get; set; }
        public string ProductDescription { get; set; }
        public string ProductDepartment { get; set; }
        public decimal ProductWeight { get; set; }
        public decimal ProductPrice { get; set; }
        public int QuantityOnHand { get; set; }
        public int MainImageNumber { get; set; }
        public bool Selected { get; set; }
    }
    public class OrderDetailMetaData
    {
    }
}