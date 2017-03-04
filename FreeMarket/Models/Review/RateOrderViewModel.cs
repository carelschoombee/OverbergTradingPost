using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FreeMarket.Models
{
    public class RateOrderViewModel
    {
        public OrderHeader Order { get; set; }
        public ProductCollection Products { get; set; }
        public List<CourierReview> CourierRatings { get; set; }
        public bool Unsubscribe { get; set; }

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

                if (Order.DeliveryType == "Courier" || Order.DeliveryType == "LocalCourier")
                {
                    Courier courier = db.Couriers.Find(Order.CourierNumber);

                    CourierRatings = db.GetAllCouriersReview(orderNumber)
                        .Select(c => new CourierReview
                        {
                            CourierNumber = c.CourierNumber,
                            ReviewContent = c.ReviewContent,
                            StarRating = c.StarRating,
                            CourierName = c.CourierName,
                            ReviewId = c.ReviewId ?? 0
                        }).ToList();

                    if (CourierRatings == null || CourierRatings.Count == 0)
                    {
                        CourierRatings = new List<CourierReview>();
                        CourierRatings.Add(new CourierReview
                        {
                            CourierNumber = courier.CourierNumber,
                            CourierName = courier.CourierName,
                            StarRating = 0,
                            ReviewContent = "",
                            ReviewId = 0
                        }
                        );
                    }
                }
                else if (Order.DeliveryType == "PostOffice")
                {
                    CourierRatings = new List<CourierReview>();
                }

                ApplicationUser user = System.Web.HttpContext.Current
                    .GetOwinContext()
                    .GetUserManager<ApplicationUserManager>()
                    .FindById(Order.CustomerNumber);

                if (user == null)
                {
                    Unsubscribe = true;
                }
                else
                {
                    Unsubscribe = user.UnsubscribeFromRatings;
                }
            }
        }
    }
}