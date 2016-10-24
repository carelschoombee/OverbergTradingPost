using FreeMarket.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FreeMarket.Controllers
{
    [RequireHttps]
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            Dashboard model = new Dashboard(null, "Year");
            return View(model);
        }

        public ActionResult EditCustomer(string customerNumber)
        {
            AspNetUserCustomer model = new AspNetUserCustomer(customerNumber, false);

            return View(model);
        }

        public ActionResult ViewCustomer(string customerNumber)
        {
            AspNetUserCustomer model = new AspNetUserCustomer(customerNumber, true);

            return View(model);
        }

        public ActionResult ViewOrderHistory(string customerNumber)
        {
            AdminOrderHistoryViewModel model = new AdminOrderHistoryViewModel(customerNumber);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchCustomer(Dashboard model)
        {
            List<AspNetUser> allUsers = new List<AspNetUser>();

            ModelState.Remove("SelectedYear");

            if (ModelState.IsValid)
            {
                List<AspNetUser> filteredUsers = AspNetUserCustomer.Filter(model.CustomerSearchCriteria);
                return PartialView("_ViewCustomers", filteredUsers);
            }
            else
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    allUsers = db.AspNetUsers.ToList();
                    return PartialView("_ViewCustomers", allUsers);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Dashboard(Dashboard data, string yearView, string monthView)
        {
            Dashboard model = new Dashboard();
            string periodType = "";

            if (!string.IsNullOrEmpty(yearView))
            {
                periodType = "Year";
            }
            else if (!string.IsNullOrEmpty(monthView))
            {
                periodType = "Month";
            }

            if (periodType == "Year")
            {
                model = new Dashboard(data.SelectedYear, periodType);
            }
            else
            {
                model = new Dashboard(data.SelectedMonth, periodType);
            }
            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkComplete(List<OrderHeader> orders)
        {
            List<OrderHeader> selected = orders
                .Where(c => c.Selected)
                .ToList();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                if (selected.Count > 0)
                {
                    foreach (OrderHeader oh in selected)
                    {
                        OrderHeader order = db.OrderHeaders.Find(oh.OrderNumber);

                        if (order != null)
                        {
                            order.OrderStatus = "Complete";
                            db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            ApplicationUser user = System.Web.HttpContext.Current
                                .GetOwinContext()
                                .GetUserManager<ApplicationUserManager>()
                                .FindById(order.CustomerNumber);

                            if (user.UnsubscribeFromRatings == false)
                            {
                                OrderHeader.SendRatingEmail(order.CustomerNumber, order.OrderNumber);
                            }
                        }
                    }
                }
            }

            return RedirectToAction("DeliverPartialTable", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkRefundComplete(List<OrderHeader> orders)
        {
            List<OrderHeader> selected = orders
                .Where(c => c.Selected)
                .ToList();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                if (selected.Count > 0)
                {
                    foreach (OrderHeader oh in selected)
                    {
                        OrderHeader order = db.OrderHeaders.Find(oh.OrderNumber);

                        if (order != null)
                        {
                            order.OrderStatus = "Refunded";
                            db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                        }
                    }

                    db.SaveChanges();
                }
            }

            return RedirectToAction("RefundCompletedPartial", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RefundOrder(int OrderNumber)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {

                OrderHeader order = db.OrderHeaders.Find(OrderNumber);

                if (order != null)
                {
                    order.OrderStatus = "RefundPending";
                    db.Entry(order).State = System.Data.Entity.EntityState.Modified;

                    OrderHeader.SendRefundEmail(order.CustomerNumber, order.OrderNumber);
                }

                db.SaveChanges();
            }

            return RedirectToAction("Index", "Admin");
        }

        public ActionResult DeliverPartialTable()
        {
            List<OrderHeader> confirmedOrders = new List<OrderHeader>();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                confirmedOrders = db.OrderHeaders.Where(c => c.OrderStatus == "Confirmed").ToList();
            }

            return PartialView("_ConfirmedOrders", confirmedOrders);
        }

        public ActionResult RefundCompletedPartial()
        {
            List<OrderHeader> pendingOrders = new List<OrderHeader>();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                pendingOrders = db.OrderHeaders.Where(c => c.OrderStatus == "RefundPending").ToList();
            }

            return PartialView("_RefundPending", pendingOrders);
        }

        public ActionResult PriceHistoryIndex()
        {
            List<PriceHistory> collection = PriceHistory.GetAllHistories();

            return View(collection);
        }

        public ActionResult SiteConfigIndex()
        {
            List<SiteConfiguration> collection = SiteConfiguration.GetSiteConfig();

            return View(collection);
        }

        public ActionResult ProductsIndex()
        {
            ProductCollection collection = ProductCollection.GetAllProducts();

            return View(collection);
        }

        public ActionResult SuppliersIndex()
        {
            SuppliersCollection collection = SuppliersCollection.GetAllSuppliers();

            return View(collection);
        }

        public ActionResult CouriersIndex()
        {
            CouriersCollection collection = CouriersCollection.GetAllCouriers();

            return View(collection);
        }

        public ActionResult SpecialsIndex()
        {
            SpecialsViewModel model = SpecialsViewModel.GetModel();

            return View(model);
        }

        public ActionResult TimeFreightIndex()
        {
            List<TimeFreightCourierFeeReference> model = Courier.GetTimeFreightPrices();

            return View(model);
        }

        public ActionResult CreateProduct()
        {
            Product product = Product.GetNewProduct();

            return View(product);
        }

        public ActionResult CreateSpecial()
        {
            Special special = Special.GetNewSpecial();

            return View(special);
        }

        public ActionResult CreateSupplier()
        {
            Supplier supplier = Supplier.GetNewSupplier();

            return View(supplier);
        }

        public ActionResult DownloadReport()
        {
            DownloadReportViewModel model = new DownloadReportViewModel();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DownloadReportProcess(DownloadReportViewModel model)
        {
            MemoryStream stream = new MemoryStream();

            if (ModelState.IsValid)
            {
                Dictionary<MemoryStream, string> obj = OrderHeader.GetOrderReport(model.OrderNumber);

                if (obj == null || obj.Count == 0)
                {
                    TempData["errorMessage"] = "An error occurred during report creation.";
                    return View("DownloadReport", model);
                }

                return File(obj.FirstOrDefault().Key, obj.FirstOrDefault().Value, string.Format("Order {0}.pdf", model.OrderNumber));
            }
            else
            {
                TempData["errorMessage"] = "That report does not exist";
                return View("DownloadReport", model);
            }
        }

        public ActionResult DownloadReportConfirmed(int orderNumber)
        {
            MemoryStream stream = new MemoryStream();

            if (ModelState.IsValid)
            {
                Dictionary<MemoryStream, string> obj = OrderHeader.GetDeliveryInstructions(orderNumber);

                if (obj == null || obj.Count == 0)
                {
                    TempData["errorMessage"] = "An error occurred during report creation.";
                    return View("Index");
                }

                return File(obj.FirstOrDefault().Key, obj.FirstOrDefault().Value, string.Format("Order {0}.pdf", orderNumber));
            }
            else
            {
                TempData["errorMessage"] = "That report does not exist";
                return View("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProductProcess(Product product, HttpPostedFileBase imagePrimary, HttpPostedFileBase imageSecondary)
        {
            if (ModelState.IsValid)
            {
                Product.CreateNewProduct(product);

                FreeMarketResult resultPrimary = FreeMarketResult.NoResult;
                FreeMarketResult resultSecondary = FreeMarketResult.NoResult;

                if (imagePrimary != null)
                    resultPrimary = Product.SaveProductImage(product.ProductNumber, PictureSize.Medium, imagePrimary);

                if (imageSecondary != null)
                    resultSecondary = Product.SaveProductImage(product.ProductNumber, PictureSize.Small, imageSecondary);

                if (resultPrimary == FreeMarketResult.Success && resultSecondary == FreeMarketResult.Success)
                    TempData["message"] = string.Format("Images uploaded and product saved for product {0}.", product.ProductNumber);

                return RedirectToAction("ProductsIndex", "Admin");
            }

            product.InitializeDropDowns("create");

            return View("CreateProduct", product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSupplierProcess(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                Supplier.CreateNewSupplier(supplier);

                return RedirectToAction("SuppliersIndex", "Admin");
            }

            return View("CreateSupplier", supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSpecialProcess(Special special)
        {
            if (ModelState.IsValid)
            {
                Special.CreateNewSpecial(special);

                return RedirectToAction("SpecialsIndex", "Admin");
            }

            return View("CreateSpecial", special);
        }

        public ActionResult ViewOrder(int orderNumber, string customerNumber)
        {
            if (string.IsNullOrEmpty(customerNumber) || orderNumber == 0)
            {
                return RedirectToAction("Index", "Admin");
            }

            OrderHeaderViewModel model = new OrderHeaderViewModel();

            model = OrderHeaderViewModel.GetOrder(orderNumber, customerNumber);

            return View(model);
        }

        public ActionResult EditProduct(int productNumber, int supplierNumber)
        {
            if (productNumber == 0 || supplierNumber == 0)
                return RedirectToAction("ProductsIndex", "Admin");

            Product product = Product.GetProduct(productNumber, supplierNumber);

            return View(product);
        }

        public ActionResult EditSpecial(int specialID)
        {
            if (specialID == 0)
                return RedirectToAction("SpecialsIndex", "Admin");

            Special special = Special.GetSpecial(specialID);

            return View(special);
        }

        public ActionResult EditSiteConfig(int siteConfigNumber)
        {
            if (siteConfigNumber == 0)
                return RedirectToAction("SiteConfigIndex", "Admin");

            SiteConfiguration config = SiteConfiguration.GetSpecificSiteConfig(siteConfigNumber);

            return View(config);
        }

        public ActionResult EditCourier(int courierNumber)
        {
            if (courierNumber == 0)
                return RedirectToAction("CouriersIndex", "Admin");

            Courier courier = Courier.GetCourier(courierNumber);

            return View(courier);
        }

        public ActionResult TimeFreightPrices(int deliveryCostID)
        {
            TimeFreightCourierFeeReference model = new TimeFreightCourierFeeReference();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                model = db.TimeFreightCourierFeeReferences.Find(deliveryCostID);
            }

            return View(model);
        }

        public ActionResult EditSupplier(int supplierNumber)
        {
            if (supplierNumber == 0)
                return RedirectToAction("SuppliersIndex", "Admin");

            Supplier supplier = Supplier.GetSupplier(supplierNumber);

            return View(supplier);
        }

        public ActionResult GetCustomerName(int orderNumber)
        {
            OrderHeader order = new OrderHeader();
            ApplicationUser user = new ApplicationUser();
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                order = db.OrderHeaders.Find(orderNumber);

                if (order == null)
                    return Content("Customer");

                user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(order.CustomerNumber);
            }

            if (user != null)
            {
                return Content(user.Name);
            }
            else
            {
                return Content("Anonymous");
            }
        }

        public ActionResult GetCustomerPhone(int orderNumber)
        {
            OrderHeader order = new OrderHeader();
            ApplicationUser user = new ApplicationUser();
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                order = db.OrderHeaders.Find(orderNumber);

                if (order == null)
                    return Content("Customer");

                user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(order.CustomerNumber);
            }

            if (user != null)
            {
                return Content(user.PhoneNumber);
            }
            else
            {
                return Content("Anonymous");
            }
        }

        public ActionResult GetCustomerEmail(int orderNumber)
        {
            OrderHeader order = new OrderHeader();
            ApplicationUser user = new ApplicationUser();
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                order = db.OrderHeaders.Find(orderNumber);

                if (order == null)
                    return Content("Customer");

                user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(order.CustomerNumber);
            }

            if (user != null)
            {
                return Content(user.Email);
            }
            else
            {
                return Content("Anonymous");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCustomerProcess(AspNetUserCustomer model)
        {
            ApplicationUser user = new ApplicationUser();
            ApplicationUserManager userManager;

            if ((string.IsNullOrEmpty(model.User.Name)) || (string.IsNullOrEmpty(model.User.Email))
                || (string.IsNullOrEmpty(model.User.PhoneNumber)) || (string.IsNullOrEmpty(model.User.SecondaryPhoneNumber))
                || (string.IsNullOrEmpty(model.SelectedCommunicationOption)))
                return View("EditCustomer", model);

            if (ModelState.IsValid)
            {
                user = System.Web.HttpContext.Current
                    .GetOwinContext()
                    .GetUserManager<ApplicationUserManager>()
                    .FindById(model.User.Id);

                userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

                if (user != null)
                {
                    user.Name = model.User.Name;
                    user.PhoneNumber = model.User.PhoneNumber;
                    user.SecondaryPhoneNumber = model.User.SecondaryPhoneNumber;
                    user.Email = model.User.Email;
                    user.PreferredCommunicationMethod = model.SelectedCommunicationOption;
                    user.UnsubscribeFromAllCorrespondence = model.User.UnsubscribeFromAllCorrespondence;

                    await userManager.UpdateAsync(user);

                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Admin");
                }
            }

            return View("EditCustomer", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProductProcess(Product product, HttpPostedFileBase imagePrimary, HttpPostedFileBase imageSecondary)
        {
            if (ModelState.IsValid)
            {
                Product.SaveProduct(product);

                FreeMarketResult resultPrimary = FreeMarketResult.NoResult;
                FreeMarketResult resultSecondary = FreeMarketResult.NoResult;

                if (imagePrimary != null)
                    resultPrimary = Product.SaveProductImage(product.ProductNumber, PictureSize.Medium, imagePrimary);

                if (imageSecondary != null)
                    resultSecondary = Product.SaveProductImage(product.ProductNumber, PictureSize.Small, imageSecondary);

                if (resultPrimary == FreeMarketResult.Success && resultSecondary == FreeMarketResult.Success)
                    TempData["message"] = string.Format("Images uploaded and product saved for product {0}.", product.ProductNumber);

                return RedirectToAction("ProductsIndex", "Admin");
            }

            product.InitializeDropDowns("edit");

            return View("EditProduct", product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSiteConfigProcess(SiteConfiguration config)
        {
            if (ModelState.IsValid)
            {
                SiteConfiguration.SaveConfig(config);

                return RedirectToAction("SiteConfigIndex", "Admin");
            }

            return View("EditSiteConfig", config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTimeFreightProcess(TimeFreightCourierFeeReference model)
        {
            if (ModelState.IsValid)
            {
                TimeFreightCourierFeeReference.SaveModel(model);

                return RedirectToAction("TimeFreightIndex", "Admin");
            }

            return View("TimeFreightPrices", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSpecialProcess(Special model)
        {
            if (ModelState.IsValid)
            {
                Special.SaveModel(model);

                return RedirectToAction("SpecialsIndex", "Admin");
            }

            return View("EditSpecial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSupplierProcess(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                Supplier.SaveSupplier(supplier);

                return RedirectToAction("SuppliersIndex", "Admin");
            }

            return View("EditSupplier", supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCourierProcess(Courier courier)
        {
            if (ModelState.IsValid)
            {
                Courier.SaveCourier(courier);

                return RedirectToAction("CouriersIndex", "Admin");
            }

            return View("EditCourier", courier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveReview(string button, int reviewId, int courierReviewId, FormCollection collection)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                ProductReview review = db.ProductReviews.Find(reviewId);
                CourierReview courierReview = db.CourierReviews.Find(courierReviewId);

                if (review == null)
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    if (button == "Approve")
                    {
                        review.Approved = true;
                    }
                    else if (button == "Revoke")
                    {
                        review.Approved = false;
                    }
                    else
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    db.Entry(review).State = System.Data.Entity.EntityState.Modified;
                }

                if (courierReview == null)
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    if (button == "Approve")
                    {
                        courierReview.Approved = true;
                    }
                    else if (button == "Revoke")
                    {
                        courierReview.Approved = false;
                    }
                    else
                    {
                        return RedirectToAction("Index", "Admin");
                    }

                    db.Entry(courierReview).State = System.Data.Entity.EntityState.Modified;
                }

                db.SaveChanges();
            }

            RatingsInfo info = new RatingsInfo();

            return RedirectToAction("Index", "Admin");
        }
    }
}