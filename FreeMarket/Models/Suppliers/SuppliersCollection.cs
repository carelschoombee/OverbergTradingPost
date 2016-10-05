using System.Collections.Generic;
using System.Linq;

namespace FreeMarket.Models
{
    public class SuppliersCollection
    {
        public List<Supplier> Suppliers { get; set; }

        public static SuppliersCollection GetAllSuppliers()
        {
            SuppliersCollection allSuppliers = new SuppliersCollection();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                allSuppliers.Suppliers = db.Suppliers.ToList();

                return allSuppliers;
            }
        }
    }
}