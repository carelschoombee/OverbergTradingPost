namespace FreeMarket.Models
{
    public class ShoppingCartViewModel
    {
        public ShoppingCart Cart { get; set; }
        public string ReturnUrl { get; set; }

        public override string ToString()
        {
            if (Cart != null && !string.IsNullOrEmpty(ReturnUrl))
                return string.Format("{0}\nReturn URL: {1}", Cart.ToString(), ReturnUrl.ToString());
            else
                return "Shopping Cart NULL";
        }
    }
}