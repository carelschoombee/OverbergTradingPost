using FreeMarket.Infrastructure;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FreeMarket.Models
{
    public class OrderHeaderViewModel
    {
        public int NumberOfItemsInOrder { get; set; }
        public OrderHeader Order { get; set; }
        public List<Courier> Couriers { get; set; }
        public bool SpecialDelivery { get; set; }

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

                model.Couriers = new List<Courier>();

                List<int?> courierNumbers = db.OrderDetails.Where(c => c.OrderNumber == order.OrderNumber)
                    .DistinctBy(c => c.CourierNumber)
                    .Select(c => c.CourierNumber)
                    .ToList();

                foreach (int courierNumber in courierNumbers)
                {
                    Courier c = db.Couriers.Find(courierNumber);
                    model.Couriers.Add(c);
                }

                int postalCode = 0;

                try
                {
                    postalCode = int.Parse(order.DeliveryAddressPostalCode);
                }
                catch (Exception e)
                {

                }

                if (db.ValidateSpecialDeliveryCode(postalCode).First() == 1)
                {
                    model.SpecialDelivery = true;
                }

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