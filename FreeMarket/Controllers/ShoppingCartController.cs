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
                Debug.Write(string.Format("\nCreating Cart for user {0} ...", userId));

                if (userId == anonymous)
                {
                    tempCart = new ShoppingCart();
                }
                else
                {
                    tempCart = new ShoppingCart(userId);
                }

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

        public CourierFeeViewModel GetCourierDataDoWork(int id, int supplierNumber, int quantity, int addressNumber, string addressString = null, bool isAjax = false)
        {
            CourierFeeViewModel model = new CourierFeeViewModel();
            string userId = User.Identity.GetUserId();
            bool anonymousUser = (userId == null);

            // Validate
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                Product product = db.Products.Find(id);
                Supplier supplier = db.Suppliers.Find(supplierNumber);

                if (product == null || supplier == null)
                    return null;
            }

            if (anonymousUser)
            {
                model = new CourierFeeViewModel(id, supplierNumber, quantity);
                return model;
            }
            else
            {
                ShoppingCart cart = GetCartFromSession(userId);
                OrderDetail detail = cart.GetOrderDetail(id, supplierNumber);
                int courierNumber = 0;

                if (!isAjax)
                {
                    if (detail == null)
                        courierNumber = 0;
                    else
                        courierNumber = (int)detail.CourierNumber;
                }

                model = new CourierFeeViewModel(id, supplierNumber, courierNumber, quantity, cart.Order.OrderNumber, userId);
                SetNoCharge(userId, model);
            }

            return model;
        }

        public ActionResult GetCourierData(int id, int supplierNumber, int quantity, int addressNumber, bool isAjax)
        {
            // Prepare
            CourierFeeViewModel model = GetCourierDataDoWork(id, supplierNumber, quantity, addressNumber, null, true);
            if (model == null)
                return RedirectToAction("Index", "Product");

            return PartialView("_CourierData", model);
        }

        public ActionResult CourierSelectionModal(int id, int supplierNumber, int quantity, int addressNumber = 0)
        {
            CourierFeeViewModel model = GetCourierDataDoWork(id, supplierNumber, quantity, addressNumber);
            if (model == null)
                return RedirectToAction("Index", "Product");

            return PartialView("_CourierSelectionModal", model);
        }

        public void SetNoCharge(string userId, CourierFeeViewModel model)
        {
            ShoppingCart cart = GetCartFromSession(userId);

            foreach (OrderDetail temp in cart.Body.OrderDetails)
            {
                foreach (CourierFee info in model.FeeInfo)
                {
                    using (FreeMarketEntities db = new FreeMarketEntities())
                    {
                        if (info.CustodianNumber == temp.CustodianNumber &&
                            info.CourierNumber == temp.CourierNumber)
                        {
                            info.NoCharge = true;
                        }
                    }
                }
            }
        }

        public void SetNoCharge(string userId, CourierFeeViewModel model, string defaultAddressName)
        {
            ShoppingCart cart = GetCartFromSession(userId);

            foreach (OrderDetail temp in cart.Body.OrderDetails)
            {
                foreach (CourierFee info in model.FeeInfo)
                {
                    using (FreeMarketEntities db = new FreeMarketEntities())
                    {
                        CustomerAddress address = db.CustomerAddresses
                            .Where(c => c.AddressName == defaultAddressName && c.CustomerNumber == userId)
                            .FirstOrDefault();

                        if (address != null)
                        {
                            if (info.CustodianNumber == temp.CustodianNumber &&
                                info.CourierNumber == temp.CourierNumber &&
                                address.AddressPostalCode == cart.Order.DeliveryAddressPostalCode)
                            {
                                info.NoCharge = true;
                            }
                        }
                    }
                }
            }
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

                if (anonymousUser)
                {
                    FreeMarketObject result;
                    result = cart.AddItemFromProduct(viewModel.ProductNumber, viewModel.SupplierNumber, viewModel.Quantity);

                    if (result.Result == FreeMarketResult.Success)
                    {
                        // New item added
                        if (result.Argument != null)
                        {
                            // If the modal was called from the shopping cart page show a different message.
                            if (viewModel.FromCart)
                                TempData["message"] = string.Format("Success: Delivery address updated for {0}.", ((Product)(result.Argument)).Description);
                            else
                                TempData["message"] = string.Format("Success: {0} ({1}) has been added to your cart.", ((Product)(result.Argument)).Description, viewModel.Quantity);
                        }
                    }
                    else
                        TempData["errorMessage"] = "Error: We could not add the item to the cart.";

                    return JavaScript("window.location = window.location.href;");
                }
                else
                {
                    // If this request comes from the ShoppingCart page then do not display quantity on the modal
                    if (viewModel.FromCart)
                        viewModel.Quantity = 0;

                    int custodian = viewModel.FeeInfo
                        .Where(c => c.CourierNumber == viewModel.SelectedCourierNumber)
                        .Select(c => c.CustodianNumber)
                        .FirstOrDefault();

                    bool noCharge = viewModel.FeeInfo
                        .Where(c => c.CourierNumber == viewModel.SelectedCourierNumber)
                        .FirstOrDefault()
                        .NoCharge;

                    FreeMarketObject result;
                    result = cart.AddItemFromProduct(viewModel.ProductNumber, viewModel.SupplierNumber, viewModel.SelectedCourierNumber, viewModel.Quantity, custodian, noCharge, userId);

                    if (result.Result == FreeMarketResult.Success)
                    {
                        // New item added
                        if (result.Argument != null)
                        {
                            // If the modal was called from the shopping cart page show a different message.
                            if (viewModel.FromCart)
                                TempData["message"] = string.Format("Success: Delivery address updated for {0}.", ((Product)(result.Argument)).Description);
                            else
                                TempData["message"] = string.Format("Success: {0} ({1}) has been added to your cart.", ((Product)(result.Argument)).Description, viewModel.Quantity);
                        }
                    }
                    else
                        TempData["errorMessage"] = "Error: We could not add the item to the cart.";

                    return JavaScript("window.location = window.location.href;");
                }
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
                    if (anonymousUser)
                    {
                        // Do nothing, the user has no courier fees assigned.
                    }
                    else
                    {
                        foreach (OrderDetail detail in selectedItems)
                        {
                            // Find an order detail which is not selected, has a courier fee of zero and a matching custodian and courier number
                            // to to the selected item. If an item is found the courier fee of the selected item is moved to the match
                            // so that it no longer has a courier fee of zero. This is to prevent a customer from getting free delivery
                            // by accident.

                            OrderDetail temp = sessionCart.Body.OrderDetails
                                    .Where(c => c.ProductNumber == detail.ProductNumber &&
                                                c.SupplierNumber == detail.SupplierNumber)
                                    .FirstOrDefault();

                            if (sessionCart.Body.OrderDetails
                                    .Where(c => !c.Selected &&
                                        c.CustodianNumber == temp.CustodianNumber &&
                                        c.CourierNumber == temp.CourierNumber &&
                                        c.CourierFee == 0)
                                    .FirstOrDefault() != null)
                            {

                                sessionCart.Body.OrderDetails
                                    .Where(c => !c.Selected &&
                                        c.CustodianNumber == temp.CustodianNumber &&
                                        c.CourierNumber == temp.CourierNumber &&
                                        c.CourierFee == 0)
                                    .FirstOrDefault().CourierFee = temp.CourierFee;
                            }

                            resultRemove = sessionCart.RemoveItem(detail.ItemNumber, detail.ProductNumber, detail.SupplierNumber, userId);
                        }
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
                {
                    TempData["message"] = "We cannot deliver all the items to your default address.";
                }
                else
                {
                    TempData["message"] = "Cart has been updated.";
                }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateDeliveryDetails(SaveCartViewModel model)
        {
            string userId = User.Identity.GetUserId();
            ShoppingCart sessionCart = GetCartFromSession(userId);

            if (ModelState.IsValid)
            {
                sessionCart.Order.UpdateDeliveryDetails(model);
                sessionCart.Save();

                if (CustomerAddress.AddressExists(userId, model.AddressName))
                {
                    FreeMarketResult result = CustomerAddress.UpdateAddress(userId, model.AddressName, model.Address.AddressLine1, model.Address.AddressLine2
                           , model.Address.AddressLine3, model.Address.AddressLine4, model.Address.AddressSuburb
                           , model.Address.AddressCity, model.Address.AddressPostalCode);

                    if (result == FreeMarketResult.Success)
                        TempData["message"] = string.Format
                            ("Your {0} delivery details have been updated.",
                            model.AddressName);
                    else
                        TempData["message"] = string.Format
                            ("Sorry, we could not process your request at this time, please try again later.");
                }
                else
                {
                    FreeMarketResult result = CustomerAddress.AddAddress(userId, model.AddressName, model.Address.AddressLine1, model.Address.AddressLine2
                           , model.Address.AddressLine3, model.Address.AddressLine4, model.Address.AddressSuburb
                           , model.Address.AddressCity, model.Address.AddressPostalCode);

                    if (result == FreeMarketResult.Success)
                        TempData["message"] = string.Format
                            ("Your {0} delivery details have been updated.",
                            model.AddressName);
                    else
                        TempData["errorMessage"] = string.Format
                            ("Sorry, we could not process your request at this time, please try again later.");
                }

                return JavaScript("window.location = window.location.href;");
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