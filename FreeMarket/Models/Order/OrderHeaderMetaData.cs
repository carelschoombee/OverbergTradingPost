using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
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

                    Debug.Write(string.Format("New order for shopping cart: {0}", order.ToString()));
                }
                // Set the customer details on the currently unconfirmed order
                else
                {
                    order.CustomerName = user.Name;
                    order.CustomerEmail = user.Email;
                    order.CustomerPrimaryContactPhone = user.PhoneNumber;
                    order.CustomerPreferredCommunicationMethod = user.PreferredCommunicationMethod;

                    Debug.Write(string.Format("Existing order for shopping cart: {0}", order.ToString()));
                }
            }

            // Return an order which can be used in a shopping cart
            return order;
        }

        public override string ToString()
        {
            string toString = "";

            if (!string.IsNullOrEmpty(CustomerName) && OrderNumber != 0)
            {
                toString += string.Format("\nOrder Header:\n");
                toString += string.Format(("\nCustomer   :    {0}"), CustomerName);
                toString += string.Format(("\nOrder      :    {0}"), OrderNumber);
                toString += string.Format(("\nTotal      :    {0}\n"), TotalOrderValue);
            }

            return toString;
        }
    }
    public class OrderHeaderMetaData
    {
        [DisplayName("Order Date Placed")]
        public DateTime OrderDatePlaced { get; set; }

        [DisplayName("Order Status")]
        public string OrderStatus { get; set; }
    }
}