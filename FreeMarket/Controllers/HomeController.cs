using System.Web.Mvc;

namespace FreeMarket.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        // GET: Product
        public ActionResult Index()
        {
            return View();
        }
    }
}