using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FreeMarket.Models
{
    public class OrderHeaderViewModel
    {
        public List<GetNumberOfItemsPerAddress_Result> DeliveryDetails { get; set; }

        public OrderHeader Order { get; set; }

        public static OrderHeaderViewModel GetOrder(int orderNumber, string userId)
        {
            OrderHeaderViewModel model = new OrderHeaderViewModel();

            if (orderNumber == 0 || string.IsNullOrEmpty(userId))
                return model;

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                OrderHeader order = db.OrderHeaders.Find(orderNumber);

                if (order == null)
                    return model;

                if (order.CustomerNumber != userId)
                    return model;

                model.Order = order;

                // Find the customer's details
                var UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var user = UserManager.FindById(userId);

                model.Order.CustomerName = user.Name;
                model.Order.CustomerEmail = user.Email;
                model.Order.CustomerPrimaryContactPhone = user.PhoneNumber;

                model.DeliveryDetails = db.GetNumberOfItemsPerAddress(model.Order.OrderNumber)
                    .ToList();

                if (model.DeliveryDetails == null || model.DeliveryDetails.Count == 0)
                    model.DeliveryDetails = new List<GetNumberOfItemsPerAddress_Result>();
            }

            return model;
        }
    }
}