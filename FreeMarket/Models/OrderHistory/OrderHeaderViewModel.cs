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
        public Courier Courier { get; set; }
        public Support Support { get; set; }
        public bool SpecialDelivery { get; set; }
        public DateTime MinDispatchDate { get; set; }
        public List<OrderDetail> ItemsInOrder { get; set; }

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

                model.ItemsInOrder = db.GetOrderDetails(orderNumber).Select(c => new OrderDetail
                {
                    ItemNumber = c.ItemNumber,
                    ProductDescription = c.Description,
                    Price = c.Price,
                    Quantity = c.Quantity,
                    SupplierName = c.SupplierName,
                    ProductWeight = c.Weight,
                    OrderItemValue = c.OrderItemValue
                }).ToList();

                model.Order = order;

                int postalCode = 0;

                model.Courier = db.Couriers.Find(order.CourierNumber);

                model.Support = db.Supports.FirstOrDefault();

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
                model.Order.CustomerPreferredCommunicationMethod = user.PreferredCommunicationMethod;

                model.NumberOfItemsInOrder = db.GetNumberOfItemsInOrder(model.Order.OrderNumber)
                    .FirstOrDefault() ?? 0;

                model.MinDispatchDate = OrderHeader.GetDispatchDay(OrderHeader.GetSuggestedDeliveryTime());
            }

            return model;
        }
    }
}