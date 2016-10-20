using System.Collections.Generic;
using System.Linq;

namespace FreeMarket.Models
{
    public class CouriersCollection
    {
        public List<Courier> Couriers { get; set; }

        public static CouriersCollection GetAllCouriers()
        {
            CouriersCollection allCouriers = new CouriersCollection();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                allCouriers.Couriers = db.Couriers.ToList();

                return allCouriers;
            }
        }
    }
}