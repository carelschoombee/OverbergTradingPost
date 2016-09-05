using FreeMarket.Models;
using System.Linq;
using System.Web.Mvc;

namespace FreeMarket.Controllers
{
    [RequireHttps]
    public class ProductController : Controller
    {
        FreeMarketEntities db = new FreeMarketEntities();

        // GET: Product
        public ActionResult Index()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult GetAllProducts()
        {
            ProductCollection products = ProductCollection.GetAllProducts();

            if (products.Products != null && products.Products.Count > 0)
                return PartialView("_ShowAllProducts", products);
            else
                return PartialView("_ShowAllProducts", new ProductCollection());
        }

        public ActionResult GetFullDescription(int productNumber, int supplierNumber)
        {
            return Content(Product.GetFullDescription(productNumber, supplierNumber));
        }


        public ActionResult GetDimensions(int productNumber)
        {
            string toReturn = "";
            Product product;

            if (productNumber != 0)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    product = db.Products
                       .Where(c => c.ProductNumber == productNumber)
                       .FirstOrDefault();

                    if (product != null)
                        toReturn = string.Format("{0} {1}", product.Weight, product.Size);
                }
            }

            return Content(toReturn);
        }
    }
}