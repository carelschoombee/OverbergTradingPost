namespace FreeMarket.Models
{
    public class ShoppingCartViewModel
    {
        public ShoppingCart Cart { get; set; }
        public string ReturnUrl { get; set; }

        public override string ToString()
        {
            return string.Format("\n{0}\n{1}\n", Cart.ToString(), ReturnUrl.ToString());
        }
    }
}