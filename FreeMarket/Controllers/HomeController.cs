using FreeMarket.Models;
using System.Linq;
using System.Web.Mvc;

namespace FreeMarket.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DeliveryOptionsInfo()
        {
            return View();
        }

        public ActionResult About()
        {
            AboutViewModel model = new AboutViewModel();

            return View(model);
        }

        public ActionResult Contact()
        {
            Support support = new Support();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                support = db.Supports.FirstOrDefault();
            }

            return View(support);
        }

        public ActionResult TermsAndConditionsModal()
        {
            string terms = "";
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                SiteConfiguration temp = db.SiteConfigurations
                    .Where(c => c.Key == "TermsAndConditions")
                    .FirstOrDefault();

                if (temp != null)
                    terms = temp.Value;
            }

            return PartialView("_TermsAndConditionsModal", terms);
        }
    }
}