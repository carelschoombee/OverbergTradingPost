using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FreeMarket.Models
{
    public class CourierFee
    {
        public int CustodianNumber { get; set; }
        public int CustodianLocationNumber { get; set; }
        public int CourierNumber { get; set; }
        public string CourierName { get; set; }
        public int QuantityOnHand { get; set; }
        public decimal DistanceBetweenCourierAndCustodian { get; set; }
        public decimal CourierFeeValue { get; set; }
        public string CustomerDestinationPostalCode { get; set; }

        public static List<CourierFee> GetCourierFees(int productNumber, int supplierNumber, int quantityRequested, int addressNumber)
        {
            List<CourierFee> feeInfo = new List<CourierFee>();
            if (productNumber == 0 || supplierNumber == 0 || quantityRequested == 0 || addressNumber == 0)
                return new List<CourierFee>();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                ProductSupplier productSupplier = db.ProductSuppliers.Find(productNumber, supplierNumber);
                if (productSupplier == null)
                {
                    Debug.Write("\nGetCourierFees::The product or supplier does not exist.");
                    return new List<CourierFee>();
                }

                CustomerAddress address = db.CustomerAddresses.Find(addressNumber);
                if (productSupplier == null)
                {
                    Debug.Write("\nGetCourierFees::The address does not exist.");
                    return new List<CourierFee>();
                }

                feeInfo = db.CalculateDeliveryFee(productNumber, supplierNumber, quantityRequested, addressNumber)
                    .OrderByDescending(c => c.CourierFee)
                    .Select(c => new CourierFee()
                    {
                        CustodianNumber = c.CustodianNumber ?? 0,
                        CustodianLocationNumber = c.CustodianLocation ?? 0,
                        CourierNumber = c.CourierNumber ?? 0,
                        CourierName = c.CourierName,
                        CustomerDestinationPostalCode = c.DestinationPostalCode,
                        CourierFeeValue = c.CourierFee ?? 0,
                        DistanceBetweenCourierAndCustodian = c.Distance ?? 0,
                        QuantityOnHand = c.QuantityOnHand ?? 0
                    }
                    ).ToList();

                if (feeInfo == null || feeInfo.Count == 0)
                {
                    Debug.Write("\nGetCourierFees::No results were returned.");
                    return new List<CourierFee>();
                }
            }

            return feeInfo;
        }
    }
}