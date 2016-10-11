using System.Collections.Generic;
using System.Linq;

namespace FreeMarket.Models
{
    public class RateOrderViewModel
    {
        public OrderHeader Order { get; set; }
        public ProductCollection Products { get; set; }
        public List<CourierReview> CourierRatings { get; set; }

        public RateOrderViewModel()
        {

        }

        public RateOrderViewModel(int orderNumber)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                Order = db.OrderHeaders.Find(orderNumber);

                if (Order == null)
                    return;

                Products = ProductCollection.GetProductsInOrder(orderNumber);

                CourierRatings = db.GetAllCouriersReview(orderNumber)
                    .Select(c => new CourierReview
                    {
                        CourierNumber = c.CourierNumber,
                        ReviewContent = c.ReviewContent,
                        StarRating = c.StarRating,
                        CourierName = c.CourierName,
                        ReviewId = c.ReviewId
                    }).ToList();
            }
        }
    }
}