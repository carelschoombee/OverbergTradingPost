using FreeMarket.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace FreeMarket.Controllers
{
    [RequireHttps]
    public class ShoppingCartController : Controller
    {
        public const string sessionKey = "cart";
        public const string anonymous = "Anonymous";

        public ShoppingCart GetCartFromSession(string userId)
        {
            userId = userId ?? anonymous;

            ShoppingCart tempCart = null;

            if (Session != null)
                tempCart = (ShoppingCart)Session[sessionKey];

            if (tempCart == null)
            {
                if (userId == anonymous)
                    tempCart = new ShoppingCart();
                else
                    tempCart = new ShoppingCart(userId);

                Session[sessionKey] = tempCart;
            }

            return tempCart;
        }

        public ActionResult Cart()
        {
            string userId = User.Identity.GetUserId();

            ShoppingCart cart = GetCartFromSession(userId);
            ShoppingCartViewModel model = new ShoppingCartViewModel();
            model = new ShoppingCartViewModel() { Cart = cart, ReturnUrl = Url.Action("Index", "Product") };

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult CartTotals(ShoppingCart cart)
        {
            return PartialView("_CartTotals", cart);
        }

        public ActionResult CourierSelectionModal(int id, int supplierNumber, int quantity)
        {
            string userId = User.Identity.GetUserId();
            ShoppingCart cart = GetCartFromSession(userId);

            CourierFeeViewModel model = new CourierFeeViewModel(id, supplierNumber, quantity, cart.Order.OrderNumber);
            if (model == null)
                return RedirectToAction("Index", "Product");

            return PartialView("_CourierSelectionModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(CourierFeeViewModel viewModel)
        {
            // Validate
            if (viewModel.ProductNumber == 0 || viewModel.SupplierNumber == 0)
            {
                TempData["errorMessage"] = "Error: We could not add the item to the cart.";
                return JavaScript("window.location = window.location.href;");
            }

            if (ModelState.IsValid)
            {
                string userId = User.Identity.GetUserId();
                bool anonymousUser = (userId == null);
                ShoppingCart cart = GetCartFromSession(userId);

                FreeMarketObject result;
                result = cart.AddItemFromProduct(viewModel.ProductNumber, viewModel.SupplierNumber, viewModel.Quantity);

                if (result.Result == FreeMarketResult.Success)
                    // New item added
                    if (!string.IsNullOrEmpty(result.Message))
                        TempData["message"] = result.Message;
                    else
                    if (!string.IsNullOrEmpty(result.Message))
                        TempData["errorMessage"] = result.Message;

                return JavaScript("window.location = window.location.href;");
            }
            // Validation Error
            else
            {
                // Prepare
                viewModel.SetInstances(viewModel.ProductNumber, viewModel.SupplierNumber);

                return PartialView("_CourierSelectionModal", viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult UpdateCart(ShoppingCart cart, string returnUrl)
        {
            // Prepare
            string userId = User.Identity.GetUserId();
            bool anonymousUser = (userId == null);
            ShoppingCart sessionCart = GetCartFromSession(userId);
            ShoppingCartViewModel model;

            if (ModelState.IsValid)
            {
                // Prepare
                FreeMarketObject resultRemove = new FreeMarketObject();
                FreeMarketObject resultQuantity = new FreeMarketObject();

                // Update Selected Property
                sessionCart.UpdateSelectedProperty(cart, false);

                // Remove selected Items
                List<OrderDetail> selectedItems = cart.Body.OrderDetails
                    .Where(c => c.Selected || c.Quantity <= 0)
                    .ToList();

                if (selectedItems.Count > 0)
                {
                    foreach (OrderDetail detail in selectedItems)
                    {
                        resultRemove = sessionCart.RemoveItem(detail.ItemNumber, detail.ProductNumber, detail.SupplierNumber, userId);
                    }
                }

                // Update Quantity
                List<OrderDetail> changedItems = cart.Body.OrderDetails
                    .Where(c => !c.Selected && c.Quantity > 0)
                    .ToList();

                if (changedItems.Count > 0)
                    resultQuantity = sessionCart.UpdateQuantities(changedItems);

                sessionCart.Save();
                sessionCart.UpdateSelectedProperty(cart, true);

                if (sessionCart.Body.OrderDetails.Any(c => c.ItemNumber == 0))
                    TempData["message"] = "We cannot deliver all the items to your default address.";
                else
                    TempData["message"] = "Cart has been updated.";

                model = new ShoppingCartViewModel { Cart = sessionCart, ReturnUrl = returnUrl };
                return RedirectToAction("Cart", "ShoppingCart");
            }

            model = new ShoppingCartViewModel { Cart = sessionCart, ReturnUrl = returnUrl };

            return View("Cart", model);
        }

        public ActionResult SaveCartModal()
        {
            string userId = User.Identity.GetUserId();
            ShoppingCart sessionCart = GetCartFromSession(userId);

            SaveCartViewModel model = new SaveCartViewModel(userId, sessionCart.Order);
            if (model == null)
                return RedirectToAction("Index", "Product");

            return PartialView("_SaveCartModal", model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateDeliveryDetails(SaveCartViewModel model)
        {
            string userId = User.Identity.GetUserId();
            ShoppingCart sessionCart = GetCartFromSession(userId);
            FreeMarketObject result;

            if (ModelState.IsValid)
            {
                sessionCart.UpdateDeliveryDetails(model);
                result = CustomerAddress.AddOrUpdateAddress(model, userId);

                if (result.Result == FreeMarketResult.Success)
                    TempData["message"] = result.Message;
                else
                    TempData["errorMessage"] = result.Message;

                return JavaScript("window.location.reload();");
            }

            model.SetAddressNameOptions(userId, model.SelectedAddress);

            return PartialView("_SaveCartModal", model);
        }

        public ActionResult UpdateCart()
        {
            string userId = User.Identity.GetUserId();

            ShoppingCart cart = GetCartFromSession(userId);
            ShoppingCartViewModel model = new ShoppingCartViewModel();
            model = new ShoppingCartViewModel() { Cart = cart, ReturnUrl = Url.Action("Index", "Product") };

            return View("Cart", model);
        }

        public ActionResult GetAddress(int AddressNumber)
        {
            string toReturn = "";
            CustomerAddress address;

            if (AddressNumber != 0)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    address = db.CustomerAddresses
                       .Where(c => c.AddressNumber == AddressNumber)
                       .FirstOrDefault();

                    if (address != null)
                        toReturn = address
                            .ToString();
                }
            }

            return Content(toReturn);
        }

        public ActionResult GetDeliveryAddress()
        {
            string toReturn = "";

            ShoppingCart cart = GetCartFromSession(User.Identity.GetUserId());
            toReturn = cart.Order.DeliveryAddress.ToString();

            return Content(toReturn);
        }

        public ActionResult GetAddressPartial(int id)
        {
            string userId = User.Identity.GetUserId();
            ShoppingCart sessionCart = GetCartFromSession(userId);
            CustomerAddress address = new CustomerAddress();

            if (id == 0)
            {
                address = new CustomerAddress
                {
                    AddressCity = sessionCart.Order.DeliveryAddressCity,
                    AddressLine1 = sessionCart.Order.DeliveryAddressLine1,
                    AddressLine2 = sessionCart.Order.DeliveryAddressLine2,
                    AddressLine3 = sessionCart.Order.DeliveryAddressLine3,
                    AddressLine4 = sessionCart.Order.DeliveryAddressLine3,
                    AddressName = "Current",
                    AddressNumber = 0,
                    AddressPostalCode = sessionCart.Order.DeliveryAddressPostalCode,
                    AddressSuburb = sessionCart.Order.DeliveryAddressSuburb,
                    CustomerNumber = userId
                };
            }
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                address = db.CustomerAddresses
                    .Where(c => c.CustomerNumber == userId && c.AddressNumber == id)
                    .FirstOrDefault();
            }

            SaveCartViewModel model = new SaveCartViewModel { Address = address, AddressName = address.AddressName };

            return PartialView("_CartModifyDeliveryDetails", model);
        }

        public ActionResult SmallCartBody()
        {
            string userId = User.Identity.GetUserId();
            ShoppingCart cart = GetCartFromSession(userId);
            ShoppingCartViewModel model = new ShoppingCartViewModel();
            model = new ShoppingCartViewModel() { Cart = cart, ReturnUrl = Url.Action("Index", "Product") };

            return PartialView("_SmallCartBody", model);
        }

        public ActionResult GetItemsCount()
        {
            string userId = User.Identity.GetUserId();
            ShoppingCart cart = GetCartFromSession(userId);

            return Content(cart.Body.OrderDetails.Count().ToString());
        }
    }
}