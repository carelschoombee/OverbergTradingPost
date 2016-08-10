using FreeMarket.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(ShoppingCart cart, int productNumber, int supplierNumber, int courierNumber, int quantity)
        {
            FreeMarketObject result;

            string userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
                result = cart.AddItemFromProduct(productNumber, supplierNumber, quantity);
            else
                result = cart.AddItemFromProduct(productNumber, supplierNumber, quantity, userId);

            if (result.Result == FreeMarketResult.Success)
            {
                if (result.Argument != null)
                {
                    TempData["message"] = string.Format("Success: {0} has been added to your cart.", ((Product)(result.Argument)).Description);
                }
            }
            else
            {
                TempData["errorMessage"] = "Error: We could not add the item to the cart.";
            }

            return JavaScript("window.location = window.location.href;");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCart(ShoppingCart cart, int itemNumber, int supplierNumber, int productNumber, string returnUrl)
        {
            FreeMarketObject result;

            string userId = User.Identity.GetUserId();

            List<OrderDetail> selectedItems = cart.Body.OrderDetails.Where(c => c.Selected).ToList();

            if (selectedItems.Count > 0)
            {
                foreach (OrderDetail detail in selectedItems)
                {
                    if (string.IsNullOrEmpty(userId))
                        result = cart.RemoveItem(itemNumber, productNumber, supplierNumber);
                    else
                        result = cart.RemoveItem(itemNumber, productNumber, supplierNumber, userId);
                }

                if (result.Result == FreeMarketResult.Success)
                {
                    if (result.Argument != null)
                    {
                        TempData["message"] = string.Format("Success: {0} has been removed from your cart.", ((Product)(result.Argument)).Description);
                    }
                }
                else
                {
                    TempData["errorMessage"] = "Error: We could not remove the item from the cart.";
                }


            }

            return new EmptyResult();
        }
    }
}