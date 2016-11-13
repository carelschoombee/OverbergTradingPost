using System.Linq;

namespace FreeMarket.Models
{
    public class WelcomeViewModel
    {
        public string WelcomeText { get; set; }

        public WelcomeViewModel()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                WelcomeText = db.SiteConfigurations.Where(c => c.Key == "IndexWelcomeText").FirstOrDefault().Value;
            }
        }
    }
}