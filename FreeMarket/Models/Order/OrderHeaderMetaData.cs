using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FreeMarket.Models
{
    [MetadataType(typeof(OrderHeaderMetaData))]
    public partial class OrderHeader
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPrimaryContactPhone { get; set; }
        public string CustomerPreferredCommunicationMethod { get; set; }

        public static OrderHeader GetOrderForShoppingCart(string customerNumber)
        {
            OrderHeader order = new OrderHeader();

            try
            {
                // Find the customer's details
                var UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var user = UserManager.FindById(customerNumber);

                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    // Determine if the customer has an unconfirmed order
                    order = db.OrderHeaders.Where(c => c.CustomerNumber == customerNumber
                                                       && c.OrderStatus == "Unconfirmed").FirstOrDefault();

                    // The customer has no unconfirmed orders
                    if (order == null)
                    {
                        order = new OrderHeader()
                        {
                            CustomerNumber = customerNumber,
                            CustomerOverallSatisfactionRating = null,
                            OrderDatePlaced = DateTime.Now,
                            OrderDateClosed = null,
                            OrderStatus = "Unconfirmed",
                            PaymentReceived = false,
                            TotalOrderValue = 0,

                            CustomerName = user.Name,
                            CustomerEmail = user.Email,
                            CustomerPrimaryContactPhone = user.PhoneNumber,
                            CustomerPreferredCommunicationMethod = user.PreferredCommunicationMethod
                        };

                        db.OrderHeaders.Add(order);
                        db.SaveChanges();
                    }
                    // Set the customer details on the currently unconfirmed order
                    else
                    {
                        order.CustomerName = user.Name;
                        order.CustomerEmail = user.Email;
                        order.CustomerPrimaryContactPhone = user.PhoneNumber;
                        order.CustomerPreferredCommunicationMethod = user.PreferredCommunicationMethod;
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionLogging.LogException(e);
            }

            // Return an order which can be used in a shopping cart
            return order;
        }
    }
    public class OrderHeaderMetaData
    {
    }
}