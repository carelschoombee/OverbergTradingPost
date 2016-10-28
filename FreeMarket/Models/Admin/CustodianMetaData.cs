using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FreeMarket.Models
{
    [MetadataType(typeof(CustodianMetaData))]
    public partial class Custodian
    {
        public ProductCollection ActivatedProducts { get; set; }

        public void InitializeActivatedProducts()
        {
            ActivatedProducts = new ProductCollection();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                ProductCollection products = new ProductCollection();
                products = ProductCollection.GetAllProducts();

                if (products == null)
                    return;

                foreach (Product item in products.Products)
                {
                    if (db.ProductCustodians.Any(c => c.ProductNumber == item.ProductNumber &&
                                                 c.SupplierNumber == item.SupplierNumber &&
                                                 c.CustodianNumber == CustodianNumber &&
                                                 c.Active == true))
                    {
                        item.CustodianActivated = true;
                        ActivatedProducts.Products.Add(item);
                    }

                    else
                    {
                        item.CustodianActivated = false;
                        ActivatedProducts.Products.Add(item);
                    }

                }
            }
        }

        public static List<Custodian> GetAllCustodians()
        {
            List<Custodian> allCustodians = new List<Custodian>();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                allCustodians = db.Custodians.ToList();

                if (allCustodians == null)
                    return new List<Custodian>();

                foreach (Custodian c in allCustodians)
                {
                    c.InitializeActivatedProducts();
                }
            }

            return allCustodians;
        }

        public static Custodian GetSpecificCustodian(int custodianNumber)
        {
            Custodian custodian = new Custodian();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                custodian = db.Custodians.Find(custodianNumber);

                if (custodian == null)
                    return new Custodian();

                custodian.InitializeActivatedProducts();
            }

            return custodian;
        }

        public static void SaveCustodian(Custodian custodian)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                db.Entry(custodian).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                foreach (Product product in custodian.ActivatedProducts.Products)
                {
                    if (product.CustodianActivated)
                    {
                        ProductCustodian productCustodian = db.ProductCustodians
                            .Where(c => c.ProductNumber == product.ProductNumber &&
                                        c.SupplierNumber == product.SupplierNumber &&
                                        c.CustodianNumber == custodian.CustodianNumber)
                                        .FirstOrDefault();
                        if (productCustodian == null)
                        {
                            productCustodian = new ProductCustodian
                            {
                                AmountLastIncreasedBySupplier = null,
                                CustodianName = custodian.CustodianName,
                                CustodianNumber = custodian.CustodianNumber,
                                DateLastIncreasedBySupplier = null,
                                ProductNumber = product.ProductNumber,
                                SupplierNumber = product.SupplierNumber,
                                QuantityOnHand = 0,
                                StockReservedForOrders = 0,
                                Active = true
                            };

                            db.ProductCustodians.Add(productCustodian);
                            db.SaveChanges();
                        }
                        else
                        {
                            productCustodian.Active = true;
                            db.Entry(productCustodian).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    else if (!product.CustodianActivated)
                    {
                        ProductCustodian productCustodian = db.ProductCustodians
                            .Where(c => c.ProductNumber == product.ProductNumber &&
                                        c.SupplierNumber == product.SupplierNumber &&
                                        c.CustodianNumber == custodian.CustodianNumber)
                                        .FirstOrDefault();
                        if (productCustodian == null)
                        {

                        }
                        else
                        {
                            productCustodian.Active = false;
                            db.Entry(productCustodian).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
            }
        }
    }

    public class CustodianMetaData
    {
        [Required]
        [StringLength(100)]
        [DisplayName("Name")]
        public string CustodianName { get; set; }

        [StringLength(50)]
        [DisplayName("Secondary Phone Number")]
        public string CustodianTelephoneNumber { get; set; }

        [StringLength(50)]
        [DisplayName("Primary Phone Number")]
        public string CustodianCellphoneNumber { get; set; }
    }
}