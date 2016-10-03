using FreeMarket.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace FreeMarket.Models
{
    public class ConfirmOrderViewModel
    {
        public ShoppingCart Cart { get; set; }
        public string Pay_Request_Id { get; set; }
        public string Checksum { get; set; }

        public List<Courier> Couriers { get; set; }

        public ConfirmOrderViewModel()
        {
            Cart = new ShoppingCart();
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