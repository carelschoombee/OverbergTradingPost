using FreeMarket.Models;
using Microsoft.AspNet.Identity;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace FreeMarket.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            WelcomeViewModel model = new WelcomeViewModel();

            if (User.Identity.Name != null)
            {
                if (User.Identity.Name == ConfigurationManager.AppSettings["developerIdentity"])
                {

                }
                else
                {
                    AuditUser.LogAudit(32, "Hit", User.Identity.GetUserId());
                }
            }
            else
            {
                AuditUser.LogAudit(32, "Hit");
            }

            return View(model);
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

        public ActionResult TermsAndConditions()
        {
            TermsAndConditionsViewModel model = new TermsAndConditionsViewModel();
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                SiteConfiguration temp = db.SiteConfigurations
                    .Where(c => c.Key == "TermsAndConditions")
                    .FirstOrDefault();

                if (temp != null)
                    model.Content = temp.Value;
            }

            return View("TermsAndConditions", model);
        }

        public ActionResult Privacy()
        {
            return View("Privacy");
        }
    }
}