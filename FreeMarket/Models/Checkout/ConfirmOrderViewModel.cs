using FreeMarket.Infrastructure;
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
        public Support Support { get; set; }

        [EnforceTrue(ErrorMessage = "You must accept the terms and conditions before you can place your order.")]
        [DisplayName("Terms and Conditions")]
        public bool TermsAndConditions { get; set; }

        public Courier Courier { get; set; }

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

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                Courier = db.Couriers.Find(cart.Order.CourierNumber);
                Support = db.Supports.FirstOrDefault();
            }
        }

        public ConfirmOrderViewModel(ShoppingCart cart)
        {
            Cart = cart;

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                Courier = db.Couriers.Find(cart.Order.CourierNumber);
                Support = db.Supports.FirstOrDefault();
            }
        }
    }
}