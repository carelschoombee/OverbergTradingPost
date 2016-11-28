using System.Collections.Generic;
using System.Linq;

namespace FreeMarket.Models
{
    public class CashCustomerViewModel
    {
        public string CustomerCriteria { get; set; }

        public static List<CashCustomer> GetCustomers(string customerCriteria)
        {
            List<CashCustomer> customers = new List<CashCustomer>();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                customers = db.FilterCashCustomers(customerCriteria)
                    .Select(c => new CashCustomer
                    {
                        DeliveryAddress = c.DeliveryAddress,
                        Email = c.Email,
                        Id = (int)c.Id,
                        Name = c.Name,
                        PhoneNumber = c.PhoneNumber
                    })
                    .ToList();
            }

            if (customers == null)
                return new List<CashCustomer>();

            return customers;
        }
    }
}