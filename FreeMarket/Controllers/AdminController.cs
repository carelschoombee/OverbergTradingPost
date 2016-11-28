using FreeMarket.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using ServiceStack;
using System;
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
        public async Task<ActionResult> Index()
        {
            Dashboard model = new Dashboard(null, "Year");
            return View(model);
        }

        public ActionResult EditCustomer(string customerNumber)
        {
            AspNetUserCustomer model = new AspNetUserCustomer(customerNumber, false);

            return View(model);
        }

        public ActionResult EditDepartment(int departmentNumber)
        {
            Department model = Department.GetDepartment(departmentNumber);

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
        public ActionResult SearchOrder(Dashboard model)
        {
            ModelState.Remove("SelectedYear");

            if (ModelState.IsValid)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    OrderHeader order = db.OrderHeaders.Find(model.OrderSearchCriteria);

                    if (order == null)
                    {
                        return Content("");
                    }

                    model.SearchedOrder = OrderHeaderViewModel.GetOrder(model.OrderSearchCriteria, order.CustomerNumber);
                }

                return PartialView("_ViewFullOrder", model.SearchedOrder);
            }
            else
            {
                return Content("");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchAudit(Dashboard model)
        {
            List<AuditUser> audits = new List<AuditUser>();

            ModelState.Remove("SelectedYear");

            if (ModelState.IsValid)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    audits = AuditUser.GetAudits(model.AuditSearchCriteria);

                    if (audits == null)
                    {
                        return Content("");
                    }
                }

                return PartialView("_ViewAudits", audits);
            }
            else
            {
                return Content("");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchCashOrders(Dashboard model)
        {
            List<CashOrderViewModel> cashOrders = new List<CashOrderViewModel>();

            ModelState.Remove("SelectedYear");

            if (ModelState.IsValid)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    cashOrders = CashOrderViewModel.GetOrders(model.CashSalesCriteria);

                    if (cashOrders == null)
                    {
                        return Content("");
                    }
                }

                return PartialView("_ViewCashOrders", cashOrders);
            }
            else
            {
                return Content("");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchCashOrderCustomers(CashCustomerViewModel model)
        {
            List<CashCustomer> cashOrders = new List<CashCustomer>();

            if (ModelState.IsValid)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    cashOrders = CashCustomerViewModel.GetCustomers(model.CustomerCriteria);

                    if (cashOrders == null)
                    {
                        return Content("");
                    }
                }

                return PartialView("_ViewCashCustomers", cashOrders);
            }
            else
            {
                return Content("");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Dashboard(Dashboard data, string yearView, string monthView)
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
                            order.OrderStatus = "Completed";
                            order.OrderDateClosed = DateTime.Now;
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

                            AuditUser.LogAudit(10, string.Format("Order Number: {0}", order.OrderNumber), User.Identity.GetUserId());
                        }
                    }
                }
            }

            return RedirectToAction("DeliverPartialInTransitTable", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkInTransit(List<OrderHeader> orders)
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
                            order.OrderStatus = "InTransit";
                            order.DateDispatched = DateTime.Now;
                            order.TrackingCodes = oh.TrackingCodes;
                            db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            ApplicationUser user = System.Web.HttpContext.Current
                                .GetOwinContext()
                                .GetUserManager<ApplicationUserManager>()
                                .FindById(order.CustomerNumber);

                            OrderHeader.SendDispatchMessage(order.CustomerNumber, order.OrderNumber);

                            AuditUser.LogAudit(37, string.Format("Order Number: {0}", order.OrderNumber), User.Identity.GetUserId());
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
                            order.DateRefunded = DateTime.Now;
                            db.Entry(order).State = System.Data.Entity.EntityState.Modified;

                            AuditUser.LogAudit(11, string.Format("Order Number: {0}", order.OrderNumber), User.Identity.GetUserId());
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
                    AuditUser.LogAudit(12, string.Format("Order Number: {0}", order.OrderNumber), User.Identity.GetUserId());
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
                confirmedOrders = db.OrderHeaders.Where(c => c.OrderStatus == "Confirmed").OrderBy(c => c.DeliveryDate).ToList();
            }

            return PartialView("_ConfirmedOrders", confirmedOrders);
        }

        public ActionResult DeliverPartialInTransitTable()
        {
            List<OrderHeader> inTransitOrders = new List<OrderHeader>();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                inTransitOrders = db.OrderHeaders.Where(c => c.OrderStatus == "InTransit").OrderBy(c => c.DeliveryDate).ToList();
            }

            return PartialView("_InTransitOrders", inTransitOrders);
        }

        public ActionResult RefundCompletedPartial()
        {
            List<OrderHeader> pendingOrders = new List<OrderHeader>();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                pendingOrders = db.OrderHeaders.Where(c => c.OrderStatus == "RefundPending").OrderBy(c => c.DeliveryDate).ToList();
            }

            return PartialView("_RefundPending", pendingOrders);
        }

        public ActionResult CustodiansIndex()
        {
            List<ProductCustodian> collection = ProductCustodian.GetAllProductCustodians();
            return View(collection);
        }

        public ActionResult CustodianActivationIndex()
        {
            List<Custodian> collection = Custodian.GetAllCustodians();
            return View(collection);
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
            ProductCollection collection = ProductCollection.GetAllProductsIncludingDeactivated();

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

        public ActionResult PostOfficeIndex()
        {
            PostOfficeViewModel model = PostOfficeViewModel.GetModel();

            return View(model);
        }

        public ActionResult DepartmentsIndex()
        {
            List<Department> model = Department.GetModel();

            return View(model);
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

        public ActionResult CashOrderIndex()
        {
            CashCustomerViewModel model = new CashCustomerViewModel();

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

        public ActionResult CreateDepartment()
        {
            Department department = Department.GetNewDepartment();

            return View(department);
        }

        public ActionResult CreateSupplier()
        {
            Supplier supplier = Supplier.GetNewSupplier();

            return View(supplier);
        }

        public ActionResult CreateCustodian()
        {
            Custodian custodian = Custodian.GetNewCustodian();

            return View(custodian);
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
            Stream stream = new MemoryStream();

            if (ModelState.IsValid)
            {
                Dictionary<Stream, string> obj = OrderHeader.GetReport(ReportType.DeliveryInstructions.ToString(), model.OrderNumber);

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
            Stream stream = new MemoryStream();

            Dictionary<Stream, string> obj = OrderHeader.GetReport(ReportType.DeliveryInstructions.ToString(), orderNumber);

            if (obj == null || obj.Count == 0)
            {
                TempData["errorMessage"] = "An error occurred during report creation.";
                return View("Index");
            }

            return File(obj.FirstOrDefault().Key, obj.FirstOrDefault().Value, string.Format("Order {0}.pdf", orderNumber));
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

                AuditUser.LogAudit(13, string.Format("Product Description: {0}", product.Description));

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

                AuditUser.LogAudit(14, string.Format("Supplier Name: {0}", supplier.SupplierName), User.Identity.GetUserId());

                return RedirectToAction("SuppliersIndex", "Admin");
            }

            return View("CreateSupplier", supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDepartmentProcess(Department department)
        {
            if (ModelState.IsValid)
            {
                Department.CreateNewDepartment(department);

                AuditUser.LogAudit(31, string.Format("Department Name: {0}", department.DepartmentName), User.Identity.GetUserId());

                return RedirectToAction("DepartmentsIndex", "Admin");
            }

            return View("CreateDepartment", department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSpecialProcess(Special special)
        {
            if (ModelState.IsValid)
            {
                Special.CreateNewSpecial(special);

                AuditUser.LogAudit(15, string.Format("Special postal code range: {0} - {1}", special.SpecialPostalCodeRangeStart, special.SpecialPostalCodeRangeEnd), User.Identity.GetUserId());

                return RedirectToAction("SpecialsIndex", "Admin");
            }

            return View("CreateSpecial", special);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCustodianProcess(ProductCustodian model, string button)
        {
            if (ModelState.IsValid)
            {
                if (button == "+ Add Stock")
                {
                    ProductCustodian.AddStock(model.ProductNumber, model.SupplierNumber, model.CustodianNumber, model.QuantityChange);
                }
                else if (button == "- Remove Stock")
                {
                    ProductCustodian.RemoveStock(model.ProductNumber, model.SupplierNumber, model.CustodianNumber, model.QuantityChange);
                }

                AuditUser.LogAudit(16, string.Format("Product Number: {0}. Supplier Number: {1}. Custodian Number: {2}", model.ProductNumber, model.SupplierNumber, model.CustodianNumber), User.Identity.GetUserId());
            }

            return RedirectToAction("CustodiansIndex", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCustodianActivationProcess(Custodian model)
        {
            if (ModelState.IsValid)
            {
                Custodian.SaveCustodian(model);

                AuditUser.LogAudit(17, string.Format("Custodian Name: {0}", model.CustodianName), User.Identity.GetUserId());
            }

            return RedirectToAction("CustodianActivationIndex", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCustodianProcess(Custodian model)
        {
            if (ModelState.IsValid)
            {
                Custodian.CreateCustodian(model);

                AuditUser.LogAudit(18, string.Format("Custodian Name: {0}", model.CustodianName), User.Identity.GetUserId());
            }

            return RedirectToAction("CustodianActivationIndex", "Admin");
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

        public ActionResult EditPostOffice(int Id)
        {
            if (Id == 0)
                return RedirectToAction("PostOfficeIndex", "Admin");

            PostalFee postalFee = PostalFee.GetPostalFee(Id);

            return View(postalFee);
        }

        public ActionResult EditSiteConfig(int siteConfigNumber)
        {
            if (siteConfigNumber == 0)
                return RedirectToAction("SiteConfigIndex", "Admin");

            SiteConfiguration config = SiteConfiguration.GetSpecificSiteConfig(siteConfigNumber);

            return View(config);
        }

        public ActionResult EditCustodian(int custodianNumber, int supplierNumber, int productNumber)
        {
            if (custodianNumber == 0 || supplierNumber == 0 || productNumber == 0)
                return RedirectToAction("CustodianIndex", "Admin");

            ProductCustodian custodian = ProductCustodian.GetSpecificCustodian(custodianNumber, supplierNumber, productNumber);

            return View(custodian);
        }

        public ActionResult EditCustodianActivation(int custodianNumber)
        {
            if (custodianNumber == 0)
                return RedirectToAction("CustodianActivationIndex", "Admin");

            Custodian custodian = Custodian.GetSpecificCustodian(custodianNumber);

            return View(custodian);
        }

        public ActionResult EditCourier(int courierNumber)
        {
            if (courierNumber == 0)
                return RedirectToAction("CouriersIndex", "Admin");

            Courier courier = Courier.GetCourier(courierNumber);

            return View(courier);
        }

        public ActionResult EditCashCustomer(int id)
        {
            if (id == 0)
                return RedirectToAction("CashOrderIndex", "Admin");

            CashCustomer customer = CashCustomer.GetCustomer(id);

            return View(customer);
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

        public ActionResult EditSupport()
        {
            Support support = Support.GetSupport();

            return View(support);
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
        public async Task<ActionResult> EditCashCustomerProcess(CashCustomer model)
        {
            if (ModelState.IsValid)
            {
                CashCustomer.SaveCustomer(model);

                AuditUser.LogAudit(38, string.Format("Customer Name: {0}", model.Name), User.Identity.GetUserId());

                return RedirectToAction("CashOrderIndex", "Admin");
            }

            return View("EditCashCustomer", model);
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

                    AuditUser.LogAudit(19, string.Format("User Name: {0}. User ID {1}.", user.Name, user.Id));

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

                AuditUser.LogAudit(20, string.Format("Product Description: {0}", product.Description), User.Identity.GetUserId());

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
                SiteConfiguration oldConfig = SiteConfiguration.SaveConfig(config);

                if (oldConfig != null)
                    AuditUser.LogAudit(21, string.Format("Key : {0}. Old Value: {1}. New Value {2}.", config.Key, oldConfig.Value, config.Value), User.Identity.GetUserId());
                else
                    AuditUser.LogAudit(21, string.Format("Key : {0}", config.Key), User.Identity.GetUserId());

                return RedirectToAction("SiteConfigIndex", "Admin");
            }

            return View("EditSiteConfig", config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSupportProcess(Support support)
        {
            if (ModelState.IsValid)
            {
                Support.SaveModel(support);

                AuditUser.LogAudit(36, "Support modified.", User.Identity.GetUserId());

                return RedirectToAction("Index", "Admin");
            }

            return View("EditSupport", support);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTimeFreightProcess(TimeFreightCourierFeeReference model)
        {
            if (ModelState.IsValid)
            {
                TimeFreightCourierFeeReference oldValue = TimeFreightCourierFeeReference.SaveModel(model);

                if (oldValue != null)
                    AuditUser.LogAudit(22, string.Format("Entry: {0}. Old Fee: {1}. New Fee: {2}", model.DeliveryCostID, oldValue.DeliveryFee, model.DeliveryFee), User.Identity.GetUserId());
                else
                    AuditUser.LogAudit(22, string.Format("Entry: {0}.", model.DeliveryCostID), User.Identity.GetUserId());

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
                Special oldSpecial = Special.SaveModel(model);

                if (oldSpecial != null)
                    AuditUser.LogAudit(23, string.Format("Entry: {0}. Old Fee: {1}. New Fee: {2}", model.SpecialID, oldSpecial.DeliveryFee, model.DeliveryFee), User.Identity.GetUserId());
                else
                    AuditUser.LogAudit(23, string.Format("Entry: {0}.", model.SpecialID), User.Identity.GetUserId());

                return RedirectToAction("SpecialsIndex", "Admin");
            }

            return View("EditSpecial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPostalFeeProcess(PostalFee model)
        {
            if (ModelState.IsValid)
            {
                PostalFee oldPostalFee = PostalFee.SaveModel(model);

                if (oldPostalFee != null)
                    AuditUser.LogAudit(23, string.Format("Entry: {0}. Old Price: {1}. Old PerKGExtraRate: {2}. New Price: {3}. New PerKGExtraRate {4}.",
                        model.Id, oldPostalFee.Price, oldPostalFee.PerKgExtraPrice, model.Price, model.PerKgExtraPrice), User.Identity.GetUserId());
                else
                    AuditUser.LogAudit(23, string.Format("Entry: {0}.", model.Id), User.Identity.GetUserId());

                return RedirectToAction("PostOfficeIndex", "Admin");
            }

            return View("EditPostOffice", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSupplierProcess(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                Supplier.SaveSupplier(supplier);

                AuditUser.LogAudit(24, string.Format("Supplier Name: {0}", supplier.SupplierName), User.Identity.GetUserId());

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

                AuditUser.LogAudit(25, string.Format("Courier Name: {0}", courier.CourierName), User.Identity.GetUserId());

                return RedirectToAction("CouriersIndex", "Admin");
            }

            return View("EditCourier", courier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDepartmentProcess(Department department)
        {
            if (ModelState.IsValid)
            {
                Department.SaveModel(department);

                AuditUser.LogAudit(30, string.Format("Department Name: {0}", department.DepartmentName), User.Identity.GetUserId());

                return RedirectToAction("DepartmentsIndex", "Admin");
            }

            return View("EditDepartment", department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveReview(string button, int reviewId, int? courierReviewId, FormCollection collection)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                ProductReview review = db.ProductReviews.Find(reviewId);

                if (courierReviewId != null)
                {
                    CourierReview courierReview = db.CourierReviews.Find(courierReviewId);

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
                }

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

                db.SaveChanges();

                AuditUser.LogAudit(26, string.Format("Review ID: {0}. Approved?: {1}", reviewId, review.Approved), User.Identity.GetUserId());
            }

            RatingsInfo info = new RatingsInfo();

            return RedirectToAction("Index", "Admin");
        }


        public ActionResult GetTotalWeightOfOrder(int orderNumber)
        {
            decimal totalWeight = 0;
            OrderHeader order = new OrderHeader();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                order = db.OrderHeaders.Find(orderNumber);

                if (order == null)
                    return Content("");

                List<OrderDetail> details = db.OrderDetails
                    .Where(c => c.OrderNumber == order.OrderNumber)
                    .ToList();

                foreach (OrderDetail detail in details)
                {
                    Product product = db.Products.Find(detail.ProductNumber);

                    if (product != null)
                    {
                        totalWeight += product.Weight * detail.Quantity;
                    }
                }
            }

            return Content(Math.Round(totalWeight, 2).ToString());
        }

        public ActionResult ExportToCsv(string id)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                var temp = OrderHeader.GetDeliveryLabels();

                string csv = String.Empty;

                id = id ?? String.Empty;

                var listCSV = temp
                        .Select(c =>
                            new
                            {
                                c.DeliveryAddress,
                                c.From,
                                c.To
                            })
                        .ToList();

                csv = listCSV.ToCsv();

                string title = string.Format("Labels {0}.csv", DateTime.Now);

                return File(new System.Text.UTF8Encoding().GetBytes(csv), "text/csv", title);
            }
        }
    }
}