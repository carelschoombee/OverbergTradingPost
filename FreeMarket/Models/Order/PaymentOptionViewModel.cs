using System.Collections.Generic;
using System.Linq;

namespace FreeMarket.Models
{
    public class PaymentOptionViewModel
    {
        public List<PaymentOption> options { get; set; }

        public PaymentOptionViewModel()
        {
            options = new List<PaymentOption>();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                options = db.PaymentOptions
                    .Where(c => c.Activated == true)
                    .ToList();
            }
        }
    }
}