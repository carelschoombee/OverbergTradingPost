using System.ComponentModel.DataAnnotations;

namespace FreeMarket.Models
{
    [MetadataType(typeof(ProductReviewMetaData))]
    public partial class ProductReview
    {
        public string ProductName { get; set; }
        public string SupplierName { get; set; }
        public string DeliveryCity { get; set; }
        public decimal TotalOrderValue { get; set; }
        public decimal ShippingTotal { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public short CourierRating { get; set; }
        public string CourierName { get; set; }
        public string CourierReviewContent { get; set; }
        public int CourierReviewId { get; set; }
    }

    public class ProductReviewMetaData
    {

    }
}