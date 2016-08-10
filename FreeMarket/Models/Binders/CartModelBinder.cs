using Microsoft.AspNet.Identity;
using System.Web;
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
                var userId = HttpContext.Current.User.Identity.GetUserId() ?? "Anonymous";

                if (userId == "Anonymous")
                    cart = new ShoppingCart();
                else
                    cart = new ShoppingCart(userId);

                if (controllerContext.HttpContext.Session != null)
                {
                    controllerContext.HttpContext.Session[SessionKey] = cart;
                }
            }

            return cart;
        }
    }
}