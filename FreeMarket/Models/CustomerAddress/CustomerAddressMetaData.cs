using System;
using System.ComponentModel.DataAnnotations;

namespace FreeMarket.Models
{
    [MetadataType(typeof(CustomerAddressMetaData))]
    public partial class CustomerAddress
    {
        public static void AddAddress(string userId, string addressName, string addressLine1, string addressLine2,
            string addressLine3, string addressLine4, string addressSuburb, string addressCity, string addressPostalCode)
        {
            try
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    CustomerAddress address = new CustomerAddress()
                    {
                        CustomerNumber = userId,
                        AddressName = addressName,
                        AddressLine1 = addressLine1,
                        AddressLine2 = addressLine2,
                        AddressLine3 = addressLine3,
                        AddressLine4 = addressLine4,
                        AddressCity = addressCity,
                        AddressPostalCode = addressPostalCode,
                        AddressSuburb = addressSuburb
                    };

                    db.CustomerAddresses.Add(address);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                ExceptionLogging.LogException(e);
            }
        }
    }

    public class CustomerAddressMetaData
    {
    }
}