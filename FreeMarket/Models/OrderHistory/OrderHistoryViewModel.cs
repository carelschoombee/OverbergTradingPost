using System.Collections.Generic;
using System.Linq;

namespace FreeMarket.Models
{
    public class OrderHistoryViewModel
    {
        public List<GetOrderHistory_Result> Items { get; set; }

        public static OrderHistoryViewModel GetOrderHistory(string userId)
        {
            List<GetOrderHistory_Result> result = new List<GetOrderHistory_Result>();

            if (!string.IsNullOrEmpty(userId))
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    result = db.GetOrderHistory(userId)
                        .ToList();
                }
            }

            OrderHistoryViewModel model = new OrderHistoryViewModel { Items = result };

            return model;
        }
    }
}