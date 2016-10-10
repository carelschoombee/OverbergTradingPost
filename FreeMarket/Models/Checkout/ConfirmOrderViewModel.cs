using FreeMarket.Infrastructure;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace FreeMarket.Models
{
    public class ConfirmOrderViewModel
    {
        public ShoppingCart Cart { get; set; }
        public string Pay_Request_Id { get; set; }
        public string Checksum { get; set; }
        public bool SpecialDelivery { get; set; }

        [EnforceTrue(ErrorMessage = "You must accept the terms and conditions before you can place your order.")]
        [DisplayName("Terms and Conditions")]
        public bool TermsAndConditions { get; set; }

        public List<Courier> Couriers { get; set; }

        public ConfirmOrderViewModel()
        {
            Cart = new ShoppingCart();
        }

        public ConfirmOrderViewModel(ShoppingCart cart, string payRequestId, string checksum, bool specialDelivery = false)
        {
            Cart = cart;
            Pay_Request_Id = payRequestId;
            Checksum = checksum;
            SpecialDelivery = specialDelivery;

            Couriers = new List<Courier>();

            List<int?> courierNumbers = Cart.Body.OrderDetails
                .DistinctBy(c => c.CourierNumber)
                .Select(c => c.CourierNumber)
                .ToList();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                foreach (int courierNumber in courierNumbers)
                {
                    Courier c = db.Couriers.Find(courierNumber);
                    Couriers.Add(c);
                }
            }
        }

        public ConfirmOrderViewModel(ShoppingCart cart)
        {
            Cart = cart;

            Couriers = new List<Courier>();

            List<int?> courierNumbers = Cart.Body.OrderDetails
                .DistinctBy(c => c.CourierNumber)
                .Select(c => c.CourierNumber)
                .ToList();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                foreach (int courierNumber in courierNumbers)
                {
                    Courier c = db.Couriers.Find(courierNumber);
                    Couriers.Add(c);
                }
            }
        }
    }
}