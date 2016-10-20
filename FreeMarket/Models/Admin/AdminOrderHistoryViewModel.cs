using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FreeMarket.Models
{
    public class AdminOrderHistoryViewModel
    {
        public OrderHistoryViewModel OrderHistory { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
        public int TotalOrders { get; set; }
        public List<GetItemHistory_Result> Quantities { get; set; }
        public List<GetDeliveryTypeHistory_Result> DeliveryTypes { get; set; }

        public AdminOrderHistoryViewModel()
        {

        }

        public AdminOrderHistoryViewModel(string customerNumber)
        {
            OrderHistory = OrderHistoryViewModel.GetOrderHistory(customerNumber);

            ApplicationUser user = System.Web.HttpContext.Current
                                .GetOwinContext()
                                .GetUserManager<ApplicationUserManager>()
                                .FindById(customerNumber);

            CustomerName = user.Name;
            CustomerNumber = customerNumber;

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                TotalOrders = db.OrderHeaders
                    .Count(c => c.CustomerNumber == customerNumber && (c.OrderStatus == "Confirmed" || c.OrderStatus == "Completed"));

                Quantities = db.GetItemHistory(customerNumber).ToList();

                DeliveryTypes = db.GetDeliveryTypeHistory(customerNumber).ToList();
            }
        }
    }
}