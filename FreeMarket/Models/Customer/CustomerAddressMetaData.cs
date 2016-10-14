using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;

namespace FreeMarket.Models
{
    [MetadataType(typeof(CustomerAddressMetaData))]
    public partial class CustomerAddress
    {
        public CustomerAddress()
        {
            CustomerNumber = "";
            AddressName = "";
            AddressLine1 = "";
            AddressLine2 = "";
            AddressLine3 = "";
            AddressLine4 = "";
            AddressCity = "";
            AddressPostalCode = "";
            AddressSuburb = "";
        }

        public static FreeMarketObject AddOrUpdateAddress(SaveCartViewModel model, string userId)
        {
            string message = "";
            FreeMarketResult result;

            if (AddressExists(userId, model.AddressName))
            {
                result = CustomerAddress.UpdateAddress(userId, model.AddressName, model.Address.AddressLine1, model.Address.AddressLine2
                      , model.Address.AddressLine3, model.Address.AddressLine4, model.Address.AddressSuburb
                      , model.Address.AddressCity, model.Address.AddressPostalCode);

                if (result == FreeMarketResult.Success)
                    message = string.Format("Your {0} address has been updated.", model.AddressName);
                else
                    message = string.Format("Your {0} address could not be updated.", model.AddressName);

            }
            else
            {
                result = CustomerAddress.AddAddress(userId, model.AddressName, model.Address.AddressLine1, model.Address.AddressLine2
                      , model.Address.AddressLine3, model.Address.AddressLine4, model.Address.AddressSuburb
                      , model.Address.AddressCity, model.Address.AddressPostalCode);

                if (result == FreeMarketResult.Success)
                    message = string.Format("Your {0} address has been added.", model.AddressName);
                else
                    message = string.Format("Your {0} address could not be added.", model.AddressName);
            }

            return new FreeMarketObject { Result = result, Message = message };
        }

        public static FreeMarketResult AddAddress(string userId, string addressName, string addressLine1, string addressLine2,
            string addressLine3, string addressLine4, string addressSuburb, string addressCity, string addressPostalCode)
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

                AuditUser.LogAudit(5, string.Format("Address name: {0}", addressName), userId);
            }

            return FreeMarketResult.Success;
        }

        public static FreeMarketResult UpdateAddress(string userId, string addressName, string addressLine1, string addressLine2,
            string addressLine3, string addressLine4, string addressSuburb, string addressCity, string addressPostalCode)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                CustomerAddress address = db.CustomerAddresses
                    .Where(c => c.CustomerNumber == userId && c.AddressName == addressName)
                    .FirstOrDefault();

                if (address == null)
                {
                    return FreeMarketResult.Failure;
                }

                address.AddressLine1 = addressLine1;
                address.AddressLine2 = addressLine2;
                address.AddressLine3 = addressLine3;
                address.AddressLine4 = addressLine4;
                address.AddressCity = addressCity;
                address.AddressPostalCode = addressPostalCode;
                address.AddressSuburb = addressSuburb;

                db.Entry(address).State = EntityState.Modified;
                db.SaveChanges();

                AuditUser.LogAudit(5, string.Format("Address name: {0}", addressName), userId);
            }

            return FreeMarketResult.Success;
        }

        public static CustomerAddress GetCustomerAddress(string userId, int addressNumber)
        {
            CustomerAddress address = new CustomerAddress();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                if (addressNumber != 0)
                    address = db.CustomerAddresses.Where(c => c.CustomerNumber == userId && c.AddressNumber == addressNumber)
                            .FirstOrDefault();
                else
                    address = db.CustomerAddresses.Where(c => c.CustomerNumber == userId)
                                    .FirstOrDefault();
            }

            if (address == null)
            {
                address = new CustomerAddress();
            }

            return address;
        }

        public static CustomerAddress GetCustomerAddress(string userId, string addressName)
        {
            CustomerAddress address = new CustomerAddress();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                if (!string.IsNullOrEmpty(addressName))
                    address = db.CustomerAddresses.Where(c => c.CustomerNumber == userId && c.AddressName == addressName)
                            .FirstOrDefault();
                else
                    address = db.CustomerAddresses.Where(c => c.CustomerNumber == userId)
                                    .FirstOrDefault();
            }

            if (address == null)
            {
                address = new CustomerAddress();
            }

            return address;
        }

        public static CustomerAddress GetCustomerAddress(int addressNumber)
        {
            CustomerAddress address = new CustomerAddress();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                address = db.CustomerAddresses.Where(c => c.AddressNumber == addressNumber)
                                    .FirstOrDefault();
            }

            if (address == null)
            {
                address = new CustomerAddress();
            }

            return address;
        }

        public static bool AddressExists(string userId, string addressName)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                return db.CustomerAddresses.Where(c => c.CustomerNumber == userId && c.AddressName == addressName && c.AddressName != "Current")
                                    .FirstOrDefault() == null ? false : true;
            }
        }

        public override string ToString()
        {
            string toReturn = "";

            toReturn += string.Format("{0}", AddressLine1);
            toReturn += string.Format("\n{0}", AddressLine2);

            if (!string.IsNullOrEmpty(AddressLine3))
                toReturn += string.Format("\n{0}", AddressLine3);

            if (!string.IsNullOrEmpty(AddressLine4))
                toReturn += string.Format("\n{0}", AddressLine4);

            toReturn += string.Format("\n{0}", AddressSuburb);
            toReturn += string.Format("\n{0}", AddressCity);
            toReturn += string.Format("\n{0}", AddressPostalCode);

            return toReturn;
        }
    }

    public class CustomerAddressMetaData
    {
        [Required]
        [Display(Name = "Address Line 1")]
        [StringLength(250, ErrorMessage = "The Address field may not contain more than 250 characters.")]
        public string AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        [StringLength(250, ErrorMessage = "The Address field may not contain more than 250 characters.")]
        public string AddressLine2 { get; set; }

        [Display(Name = "Address Line 3")]
        [StringLength(250, ErrorMessage = "The Address field may not contain more than 250 characters.")]
        public string AddressLine3 { get; set; }

        [Display(Name = "Address Line 4")]
        [StringLength(250, ErrorMessage = "The Address field may not contain more than 250 characters.")]
        public string AddressLine4 { get; set; }

        [Display(Name = "Suburb")]
        [StringLength(50, ErrorMessage = "The Suburb field may not contain more than 50 characters.")]
        public string AddressSuburb { get; set; }

        [Required]
        [Display(Name = "City")]
        [StringLength(50, ErrorMessage = "The City field may not contain more than 50 characters.")]
        public string AddressCity { get; set; }

        [Required]
        [Display(Name = "Postal Code")]
        [StringLength(4, ErrorMessage = "The Postal Code field may not contain more than 4 digits.")]
        [RegularExpression(@"^(\d{4})$", ErrorMessage = "The postal code must consist only of digits.")]
        public string AddressPostalCode { get; set; }
    }
}