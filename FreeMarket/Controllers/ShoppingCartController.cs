using FreeMarket.Models;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;

namespace FreeMarket.Controllers
{
    [RequireHttps]
    [FreeMarketErrorHandler(View = "Error")]
    public class ShoppingCartController : Controller
    {
        public ActionResult Cart()
        {
            ShoppingCartViewModel model = new ShoppingCartViewModel();

            string UserId = User.Identity.GetUserId();

            if (UserId == null)
            {
                // User not logged in, use Cookie
            }

            ShoppingCart cart = new ShoppingCart(UserId);
            model = new ShoppingCartViewModel() { Cart = cart };

            return View(model);
        }

        public ActionResult CartTotals(ShoppingCart cart)
        {
            return PartialView("_CartTotals", cart);
        }
    }
}