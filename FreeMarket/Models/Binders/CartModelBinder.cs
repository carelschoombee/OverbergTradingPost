using System.Web.Mvc;

namespace FreeMarket.Models
{
    public class CartModelBinder : IModelBinder
    {
        private const string SessionKey = "Cart";

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            ShoppingCart cart = null;

            if (controllerContext.HttpContext.Session != null)
            {
                cart = (ShoppingCart)controllerContext.HttpContext.Session[SessionKey];
            }

            if (cart == null)
            {
                cart = new ShoppingCart();

                if (controllerContext.HttpContext.Session != null)
                {
                    controllerContext.HttpContext.Session[SessionKey] = cart;
                }
            }

            return cart;
        }
    }
}