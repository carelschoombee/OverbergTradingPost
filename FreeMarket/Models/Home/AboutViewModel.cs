using System.Linq;

namespace FreeMarket.Models
{
    public class AboutViewModel
    {
        public string MainDescription { get; set; }

        public AboutViewModel()
        {
            string description = "";
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                description = db.SiteConfigurations.Where(c => c.Key == "AboutUs").FirstOrDefault().Value;

                if (!string.IsNullOrEmpty(description))
                {
                    MainDescription = description;
                }
                else
                {
                    MainDescription = "";
                }
            }
        }
    }
}