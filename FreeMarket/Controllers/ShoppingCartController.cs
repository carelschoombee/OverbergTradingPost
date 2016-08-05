using FreeMarket.Models;
using Microsoft.AspNet.Identity;
using System.Diagnostics;
using System.Web.Mvc;

namespace FreeMarket.Controllers
{
    [RequireHttps]
    public class ShoppingCartController : Controller
    {
        public ActionResult Cart(ShoppingCart tempCart)
        {
            ShoppingCartViewModel model = new ShoppingCartViewModel();
            ShoppingCart cart;

            string userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                Debug.Write(string.Format("\nCreating Cart in Session..."));

                model = new ShoppingCartViewModel() { Cart = tempCart };
            }
            else
            {
                Debug.Write(string.Format("\nMerging Cart with Database..."));

                cart = new ShoppingCart(userId);
                cart.Merge(tempCart, userId);
                model = new ShoppingCartViewModel() { Cart = cart };
            }

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult CartTotals(ShoppingCart cart)
        {
            return PartialView("_CartTotals", cart);
        }
    }
}