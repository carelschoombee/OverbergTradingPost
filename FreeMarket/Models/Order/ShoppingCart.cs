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

        public void Save() { }

        public void AddItem(OrderDetail item) { }
        public void RemoveItem(OrderDetail item) { }

        public void UpdatePrices() { }

        public void Checkout() { }

        public List<string> CreateCookie() { return new List<string>(); }

        public ShoppingCart(string userId)
        {
            Initialize(userId);
        }

        public ShoppingCart()
        {

        }
    }
}