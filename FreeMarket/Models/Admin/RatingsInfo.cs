using System.Collections.Generic;
using System.Linq;

namespace FreeMarket.Models
{
    public class RatingsInfo
    {
        public Dictionary<Product, Dictionary<string, int>> AverageRatings { get; set; }
        public Dictionary<Courier, int> CourierRatings { get; set; }
        public List<ProductReview> ProductRatings { get; set; }

        public RatingsInfo()
        {
            ProductRatings = new List<ProductReview>();
            AverageRatings = new Dictionary<Product, Dictionary<string, int>>();
            CourierRatings = new Dictionary<Courier, int>();

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

                if (ProductRatings == null)
                    ProductRatings = new List<ProductReview>();

                List<ProductSupplier> products = db.ProductSuppliers.ToList();

                if (products != null && products.Count > 0)
                {
                    foreach (ProductSupplier product in products)
                    {
                        Product fullProduct = Product.GetProduct(product.ProductNumber, product.SupplierNumber);

                        if (fullProduct != null && fullProduct.Activated == true)
                        {
                            double qualityRating = db.ProductReviews
                                .Where(c => c.ProductNumber == product.ProductNumber && c.SupplierNumber == product.SupplierNumber)
                                .Average(m => m.StarRating) ?? 0;

                            double priceRating = db.ProductReviews
                                .Where(c => c.ProductNumber == product.ProductNumber && c.SupplierNumber == product.SupplierNumber)
                                .Average(m => m.PriceRating) ?? 0;

                            int countReviews = db.ProductReviews
                                .Where(c => c.ProductNumber == product.ProductNumber && c.SupplierNumber == product.SupplierNumber)
                                .Count();

                            fullProduct.ProductReviewsCount = countReviews;

                            if (!AverageRatings.ContainsKey(fullProduct))
                            {
                                Dictionary<string, int> qualityInfo = new Dictionary<string, int>();
                                qualityInfo.Add("Quality", (int)qualityRating);

                                AverageRatings.Add(fullProduct, qualityInfo);

                                Dictionary<string, int> priceInfo = new Dictionary<string, int>();
                                priceInfo.Add("Price", (int)priceRating);

                                AverageRatings[fullProduct].Add("Price", (int)priceRating);
                            }
                        }
                    }
                }

                List<CourierReview> courierReviews = db.GetAllCouriersReviewList()
                    .Select(c => new CourierReview
                    {
                        StarRating = c.StarRating,
                        CourierNumber = c.CourierNumber,
                        CourierName = c.CourierName
                    })
                    .ToList();

                foreach (Courier courier in db.Couriers)
                {
                    double courierRating = courierReviews
                                .Where(c => c.CourierNumber == courier.CourierNumber)
                                .Average(m => m.StarRating) ?? 0;

                    int countCouriers = db.CourierReviews
                                .Where(c => c.CourierNumber == courier.CourierNumber)
                                .Count();

                    courier.CourierReviewsCount = countCouriers;

                    if (!CourierRatings.ContainsKey(courier))
                    {
                        CourierRatings.Add(courier, (int)courierRating);
                    }
                    else
                    {
                        CourierRatings[courier] = (int)courierRating;
                    }
                }
            }
        }
    }
}