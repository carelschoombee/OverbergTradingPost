using System.Linq;

namespace FreeMarket.Models
{
    public class DeliveryOptionsViewModel
    {
        public string CourierDeliveryDescription { get; set; }
        public string PostOfficeDeliveryDescription { get; set; }

        public DeliveryOptionsViewModel()
        {
            string courierDescription = "";
            string postOfficeDescription = "";
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                courierDescription = db.SiteConfigurations.Where(c => c.Key == "CourierDeliveryOptionDescription").FirstOrDefault().Value;
                postOfficeDescription = db.SiteConfigurations.Where(c => c.Key == "PostOfficeDeliveryOptionDescription").FirstOrDefault().Value;

                if (!string.IsNullOrEmpty(courierDescription))
                {
                    CourierDeliveryDescription = courierDescription;
                }
                else
                {
                    CourierDeliveryDescription = "";
                }

                if (!string.IsNullOrEmpty(postOfficeDescription))
                {
                    PostOfficeDeliveryDescription = postOfficeDescription;
                }
                else
                {
                    PostOfficeDeliveryDescription = "";
                }
            }
        }
    }
}