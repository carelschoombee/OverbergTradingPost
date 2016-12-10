using System.Linq;

namespace FreeMarket.Models
{
    public class SpecialMessageViewModel
    {
        public string SpecialMessage { get; set; }

        public SpecialMessageViewModel()
        {
            string message = "";
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                message = db.SiteConfigurations.Where(c => c.Key == "SpecialMessage").FirstOrDefault().Value;

                if (!string.IsNullOrEmpty(message))
                {
                    SpecialMessage = message;
                }
                else
                {
                    SpecialMessage = "";
                }
            }
        }
    }
}