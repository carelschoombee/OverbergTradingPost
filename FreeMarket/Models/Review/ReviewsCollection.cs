using System.Collections.Generic;
using System.Linq;

namespace FreeMarket.Models
{
    public class ProductReviewsCollection
    {
        public List<ProductReview> Reviews { get; set; }
        public double AverageReviewRating { get; set; }

        public ProductReviewsCollection(int productNumber, int supplierNumber)
        {
            List<ProductReview> reviews = new List<ProductReview>();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                reviews = db.ProductReviews
                    .Where(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber)
                    .ToList();
            }

            Reviews = reviews;
            AverageReviewRating = CalculateAverageRating() ?? 0;
        }

        public double? CalculateAverageRating()
        {
            return Reviews.Average(m => m.StarRating);
        }
    }
}