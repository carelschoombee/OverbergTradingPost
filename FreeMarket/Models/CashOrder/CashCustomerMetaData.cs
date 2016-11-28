using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FreeMarket.Models
{
    [MetadataType(typeof(CashCustomerMetaData))]
    public partial class CashCustomer
    {
        public static CashCustomer GetCustomer(int id)
        {
            CashCustomer customer = new CashCustomer();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                customer = db.CashCustomers.Find(id);
            }

            return customer;
        }
    }

    public class CashCustomerMetaData
    {
        [Required]
        public string Name { get; set; }

        [DisplayName("Delivery Address")]
        [StringLength(1100)]
        public string DeliveryAddress { get; set; }

        public string Email { get; set; }

        [StringLength(256)]
        public string PhoneNumber { get; set; }
    }
}