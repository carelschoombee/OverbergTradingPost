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
        public bool NoCharge { get; set; }

        public static List<CourierFee> GetCourierFees(int productNumber, int supplierNumber, int quantityRequested, int orderNumber)
        {
            // Validate
            List<CourierFee> feeInfo = new List<CourierFee>();
            if (productNumber == 0 || supplierNumber == 0 || quantityRequested == 0)
                return feeInfo;

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                // Validate
                ProductSupplier productSupplier = db.ProductSuppliers.Find(productNumber, supplierNumber);
                if (productSupplier == null)
                {
                    Debug.Write("\nGetCourierFees::The product or supplier does not exist.");
                    return feeInfo;
                }

                // Calculate potential fees for each courier
                feeInfo = db.CalculateDeliveryFee(productNumber, supplierNumber, quantityRequested, orderNumber)
                    .Select(c => new CourierFee()
                    {
                        CustodianNumber = c.CustodianNumber ?? 0,
                        CustodianLocationNumber = c.CustodianLocation ?? 0,
                        CourierNumber = c.CourierNumber ?? 0,
                        CourierName = c.CourierName,
                        CustomerDestinationPostalCode = c.DestinationPostalCode,
                        CourierFeeValue = c.CourierFee ?? 0,
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

        public override string ToString()
        {
            string toString = "";

            toString += string.Format(("\nCustodian Number                        : {0}"), CustodianNumber);
            toString += string.Format(("\nCustodian Location Number               : {0}"), CustodianLocationNumber);
            toString += string.Format(("\nCourier Number                          : {0}"), CourierNumber);
            toString += string.Format(("\nCourier Name                            : {0}"), CourierName);
            toString += string.Format(("\nQuantity On Hand                        : {0}"), QuantityOnHand);
            toString += string.Format(("\nDistance Between Courier And Custodian  : {0}"), DistanceBetweenCourierAndCustodian);
            toString += string.Format(("\nCourier Fee Value                       : {0}"), CourierFeeValue);
            toString += string.Format(("\nCustomer Destination PostalCode         : {0}"), CustomerDestinationPostalCode);

            return toString;
        }
    }
}