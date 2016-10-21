using System.Collections.Generic;
using System.Linq;

namespace FreeMarket.Models
{
    public class ProductReviewsCollection
    {
        public int ProductNumber { get; set; }
        public int SupplierNumber { get; set; }
        public int PageSize { get; set; }
        public List<ProductReview> Reviews { get; set; }

        public static double? CalculateAverageRatingOnly(int productNumber, int supplierNumber)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                double qualityRating = db.ProductReviews
                    .Where(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber && c.Approved == true)
                    .Average(m => m.StarRating) ?? 0;

                double priceRating = db.ProductReviews
                    .Where(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber && c.Approved == true)
                    .Average(m => m.PriceRating) ?? 0;

                return (qualityRating + priceRating) / 2;
            }
        }

        public static ProductReviewsCollection GetReviewsOnly(int productNumber, int supplierNumber, int size)
        {
            ProductReviewsCollection collection = new ProductReviewsCollection();
            List<ProductReview> reviews = new List<ProductReview>();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                reviews = db.ProductReviews
                    .Where(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber && c.Approved == true)
                    .OrderByDescending(c => c.StarRating)
                    .Take(size)
                    .ToList();
            }

            collection.Reviews = reviews;
            collection.ProductNumber = productNumber;
            collection.SupplierNumber = supplierNumber;
            collection.PageSize = size;

            return collection;
        }
    }
}