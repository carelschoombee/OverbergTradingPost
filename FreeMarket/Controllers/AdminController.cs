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

                if (imagePrimary != null)
                    Product.SaveProductImage(product.ProductNumber, "256x192", imagePrimary);

                if (imageSecondary != null)
                    Product.SaveProductImage(product.ProductNumber, "80x79", imageSecondary);

                return RedirectToAction("ProductsIndex", "Admin");
            }

            return View(product);
        }
    }
}