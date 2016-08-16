using System.Collections.Generic;
using System.Linq;

namespace FreeMarket.Models
{
    public class PurchaseAlgorithms
    {
        public static int WhereDoWeDeliverTo(int addressNumber)
        {
            return 0;
        }

        public static decimal CalculateDeliveryPrice(int deliveryAddress, int courierNumber, int custodianNumber)
        {
            return 0;
        }

        public static Dictionary<Courier, decimal> ExecuteAlgorithm(int productNumber, int supplierNumber, int courierNumber, int quantityRequested, int addressNumber)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                List<Courier> couriers = db.Couriers.ToList();
                if (couriers == null || couriers.Count == 0)
                {
                    return new Dictionary<Courier, decimal>();
                }

                foreach (Courier c in couriers)
                {
                    var custodianInfo = db.GetLowestDistanceBetweenCourierAndCustodian(productNumber, supplierNumber, c.CourierNumber, quantityRequested, addressNumber)
                        .Select(x => new
                        {
                            CustodianNumber = x.CustodianNumber,
                            CourierNumber = x.CourierNumber,
                            Distance = x.Distance,
                        }
                        );
                }

            }


            return new Dictionary<Courier, decimal>();
        }
    }
}