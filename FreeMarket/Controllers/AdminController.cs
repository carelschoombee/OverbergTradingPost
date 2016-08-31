using FreeMarket.Models;
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

        public ActionResult CreateProduct()
        {
            Product product = new Product();

            return View(product);
        }

        public ActionResult EditProduct(int productNumber, int supplierNumber)
        {
            if (productNumber == 0 || supplierNumber == 0)
                return RedirectToAction("ProductsIndex", "Admin");

            Product product = Product.GetProduct(productNumber, supplierNumber);

            return View(product);
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
                {
                    TempData["message"] = string.Format("Images uploaded and product saved for product {0}.", product.ProductNumber);
                }

                return RedirectToAction("ProductsIndex", "Admin");
            }

            return View(product);
        }
    }
}