using FreeMarket.Models;
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
    }
}