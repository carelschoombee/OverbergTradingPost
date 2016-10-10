using System.Collections.Generic;

namespace FreeMarket.Models
{
    public class RateOrderViewModel
    {
        public OrderHeader Order { get; set; }
        public ProductCollection Products { get; set; }
        public Dictionary<int, int> ProductRatings { get; set; }
        public Dictionary<int, int> PriceRatings { get; set; }
        public Dictionary<int, int> CourierRatings { get; set; }

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
            }
        }
    }
}