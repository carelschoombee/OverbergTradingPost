using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FreeMarket.Models
{
    [MetadataType(typeof(ProductCustodianMetaData))]
    public partial class ProductCustodian
    {
        public string CustodianName { get; set; }
        public string SupplierName { get; set; }
        public string ProductName { get; set; }

        [DisplayName("Amount of stock to be added / removed:")]
        public int QuantityChange { get; set; }

        public static ProductCustodian GetSpecificCustodian(int custodianNumber, int supplierNumber, int productNumber)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                return db.GetAllProductCustodians()
                    .Where(c => c.CustodianNumber == custodianNumber && c.SupplierNumber == supplierNumber && c.ProductNumber == productNumber)
                    .Select(c => new ProductCustodian
                    {
                        QuantityOnHand = c.QuantityOnHand,
                        AmountLastIncreasedBySupplier = c.AmountLastIncreasedBySupplier,
                        CustodianName = c.CustodianName,
                        CustodianNumber = c.CustodianNumber,
                        DateLastIncreasedBySupplier = c.DateLastIncreasedBySupplier,
                        ProductNumber = c.ProductNumber,
                        StockReservedForOrders = c.StockReservedForOrders,
                        SupplierNumber = c.SupplierNumber,
                        ProductName = c.Description,
                        SupplierName = c.SupplierName,
                        QuantityChange = 0
                    }).FirstOrDefault();
            }
        }

        public static List<ProductCustodian> GetAllProductCustodians()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                return db.GetAllProductCustodians().Select(c => new ProductCustodian
                {
                    QuantityOnHand = c.QuantityOnHand,
                    AmountLastIncreasedBySupplier = c.AmountLastIncreasedBySupplier,
                    CustodianName = c.CustodianName,
                    CustodianNumber = c.CustodianNumber,
                    DateLastIncreasedBySupplier = c.DateLastIncreasedBySupplier,
                    ProductNumber = c.ProductNumber,
                    StockReservedForOrders = c.StockReservedForOrders,
                    SupplierNumber = c.SupplierNumber,
                    ProductName = c.Description,
                    SupplierName = c.SupplierName,
                    QuantityChange = 0
                }).ToList();
            }
        }

        public static void AddStock(int productNumber, int supplierNumber, int custodianNumber, int quantity)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                ProductCustodian custodian = db.ProductCustodians.Where(c => c.CustodianNumber == custodianNumber &&
                                                c.ProductNumber == productNumber &&
                                                c.SupplierNumber == supplierNumber)
                                                .FirstOrDefault();

                custodian.QuantityOnHand += quantity;
                custodian.DateLastIncreasedBySupplier = DateTime.Now;
                custodian.AmountLastIncreasedBySupplier = quantity;

                db.Entry(custodian).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static void RemoveStock(int productNumber, int supplierNumber, int custodianNumber, int quantity)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                ProductCustodian custodian = db.ProductCustodians.Where(c => c.CustodianNumber == custodianNumber &&
                                                c.ProductNumber == productNumber &&
                                                c.SupplierNumber == supplierNumber)
                                                .FirstOrDefault();

                custodian.QuantityOnHand -= quantity;
                custodian.DateLastIncreasedBySupplier = DateTime.Now;
                custodian.AmountLastIncreasedBySupplier = quantity;

                if (custodian.QuantityOnHand < 0)
                    custodian.QuantityOnHand = 0;

                db.Entry(custodian).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }
    }

    public class ProductCustodianMetaData
    {
        [DisplayName("Custodian Name")]
        public string CustodianName { get; set; }

        [DisplayName("Product Name")]
        public string ProductName { get; set; }

        [DisplayName("Supplier Name")]
        public string SupplierName { get; set; }

        [DisplayName("Quantity On Hand")]
        public int QuantityOnHand { get; set; }

        [DisplayName("Stock Reserved")]
        public int StockReservedForOrders { get; set; }

        [DisplayName("Last Modified")]
        public DateTime DateLastIncreasedBySupplier { get; set; }

        [DisplayName("Last Modified Amount")]
        public int AmountLastIncreasedBySupplier { get; set; }
    }
}