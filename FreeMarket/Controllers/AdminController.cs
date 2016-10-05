using FreeMarket.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            return View();
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

        public ActionResult CreateProduct()
        {
            Product product = Product.GetNewProduct();

            return View(product);
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

        public ActionResult EditProduct(int productNumber, int supplierNumber)
        {
            if (productNumber == 0 || supplierNumber == 0)
                return RedirectToAction("ProductsIndex", "Admin");

            Product product = Product.GetProduct(productNumber, supplierNumber);

            return View(product);
        }

        public ActionResult EditSupplier(int supplierNumber)
        {
            if (supplierNumber == 0)
                return RedirectToAction("SuppliersIndex", "Admin");

            Supplier supplier = Supplier.GetSupplier(supplierNumber);

            return View(supplier);
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
        public ActionResult EditSupplierProcess(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                Supplier.SaveSupplier(supplier);

                FreeMarketResult resultPrimary = FreeMarketResult.NoResult;
                FreeMarketResult resultSecondary = FreeMarketResult.NoResult;

                if (imagePrimary != null)
                    resultPrimary = Product.SaveProductImage(supplier.ProductNumber, PictureSize.Medium, imagePrimary);

                if (imageSecondary != null)
                    resultSecondary = Product.SaveProductImage(supplier.ProductNumber, PictureSize.Small, imageSecondary);

                if (resultPrimary == FreeMarketResult.Success && resultSecondary == FreeMarketResult.Success)
                    TempData["message"] = string.Format("Images uploaded and product saved for product {0}.", supplier.ProductNumber);

                return RedirectToAction("ProductsIndex", "Admin");
            }

            supplier.InitializeDropDowns("edit");

            return View("EditProduct", supplier);
        }
    }
}