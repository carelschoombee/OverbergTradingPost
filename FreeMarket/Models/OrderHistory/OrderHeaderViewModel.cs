using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Linq;
using System.Web;

namespace FreeMarket.Models
{
    public class OrderHeaderViewModel
    {
        public int NumberOfItemsInOrder { get; set; }
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

                model.NumberOfItemsInOrder = db.GetNumberOfItemsInOrder(model.Order.OrderNumber)
                    .Select(c => c.Value)
                    .FirstOrDefault();
            }

            return model;
        }
    }
}