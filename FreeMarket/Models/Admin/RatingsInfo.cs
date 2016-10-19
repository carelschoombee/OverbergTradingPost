using System.Collections.Generic;
using System.Linq;

namespace FreeMarket.Models
{
    public class RatingsInfo
    {
        public Dictionary<string, int> AverageRatings { get; set; }
        public List<ProductReview> ProductRatings { get; set; }

        public RatingsInfo()
        {
            ProductRatings = new List<ProductReview>();
            AverageRatings = new Dictionary<string, int>();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                ProductRatings = db.GetAllProductsReview()
                    .Select(c => new ProductReview
                    {
                        Approved = c.Approved,
                        Author = c.Author,
                        CourierName = c.CourierName,
                        CourierRating = c.CourierRating ?? 0,
                        CourierReviewId = (int)c.CourierReviewId,
                        CourierReviewContent = c.CourierRatingReview,
                        Date = c.Date,
                        DeliveryCity = c.DeliveryAddressCity,
                        OrderNumber = c.OrderNumber,
                        Price = c.Price,
                        PriceRating = c.PriceRating,
                        ProductName = c.Description,
                        ProductNumber = c.ProductNumber,
                        Quantity = c.Quantity,
                        ReviewContent = c.ReviewContent,
                        ReviewId = c.ReviewId,
                        ShippingTotal = c.ShippingTotal ?? 0,
                        StarRating = c.StarRating,
                        SupplierName = c.SupplierName,
                        SupplierNumber = c.SupplierNumber,
                        TotalOrderValue = c.TotalOrderValue,
                        UserId = c.UserId

                    }).ToList();

                if (ProductRatings.Count > 0)
                {
                    List<ProductSupplier> products = db.ProductSuppliers.ToList();

                    if (products != null && products.Count > 0)
                    {
                        foreach (ProductSupplier product in products)
                        {
                            double qualityRating = ProductRatings
                                .Where(c => c.ProductNumber == product.ProductNumber && c.SupplierNumber == product.SupplierNumber)
                                .Average(m => m.StarRating) ?? 0;

                            double priceRating = ProductRatings
                                .Where(c => c.ProductNumber == product.ProductNumber && c.SupplierNumber == product.SupplierNumber)
                                .Average(m => m.PriceRating) ?? 0;
                        }
                    }
                }
            }
        }
    }
}