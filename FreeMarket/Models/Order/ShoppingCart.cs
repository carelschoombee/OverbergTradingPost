using System.Collections.Generic;

namespace FreeMarket.Models
{
    public class ShoppingCart
    {
        public OrderHeader Order { get; set; }
        public CartBody Body { get; set; }

        public void Initialize(string userId)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                Order = OrderHeader.GetOrderForShoppingCart(userId);
                Body = CartBody.GetDetailsForShoppingCart(Order.OrderNumber);
            }
        }

        public void AddItem(OrderDetail item)
        {
            // Remember to handle the case an existing item is added to the same order again.
            // Increment quantity only.
        }

        public void RemoveItem(OrderDetail item) { }

        public void UpdatePrices() { }

        public void Save() { }

        public void Checkout() { }

        public List<string> CreateCookie() { return new List<string>(); }

        public ShoppingCart(string userId)
        {
            Initialize(userId);
        }

        public ShoppingCart() { }

        public override string ToString()
        {
            string toString = "";

            toString += "Shopping Cart Contents:";
            toString += string.Format("{0}{1}", Order.ToString(), Body.ToString());

            return toString;
        }
    }
}