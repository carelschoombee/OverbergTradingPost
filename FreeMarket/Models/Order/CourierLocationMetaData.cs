using System.Collections.Generic;

namespace FreeMarket.Models
{
    public class CourierLocationMetaData
    {
        public string CourierName { get; set; }
        public string PostalCodeOrigin { get; set; }
        public string PostalCodeDestination { get; set; }

        public static List<CourierLocation> GetCourierLocations(string customerPostalCode, string supplierPostalCode)
        {
            if (string.IsNullOrEmpty(customerPostalCode) || string.IsNullOrEmpty(supplierPostalCode))
                return new List<CourierLocation>();

            
        }
    }
}