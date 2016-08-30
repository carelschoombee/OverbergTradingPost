using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace FreeMarket.Models
{
    [MetadataType(typeof(ProductMetaData))]
    public partial class Product
    {
        public int MainImageNumber { get; set; }
        public int SecondaryImageNumber { get; set; }

        [DisplayName("Supplier Number")]
        public int SupplierNumber { get; set; }

        [DisplayName("Supplier Name")]
        public string SupplierName { get; set; }

        [DisplayName("Department Name")]
        public string DepartmentName { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [DisplayName("Price Per Unit")]
        public decimal PricePerUnit { get; set; }

        public static Product GetProduct(int productNumber, int supplierNumber)
        {
            Product product = new Product();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                var productInfo = db.GetProduct(productNumber, supplierNumber)
                    .FirstOrDefault();

                product = new Product
                {
                    Activated = productInfo.Activated,
                    DateAdded = productInfo.DateAdded,
                    DateModified = productInfo.DateModified,
                    DepartmentName = productInfo.DepartmentName,
                    DepartmentNumber = productInfo.DepartmentNumber,
                    Description = productInfo.Description,
                    PricePerUnit = productInfo.PricePerUnit,
                    ProductNumber = productInfo.ProductNumberID,
                    Size = productInfo.Size,
                    SupplierName = productInfo.SupplierName,
                    SupplierNumber = productInfo.SupplierNumberID,
                    Weight = productInfo.Weight
                };

                product.MainImageNumber = db.ProductPictures
                    .Where(c => c.ProductNumber == product.ProductNumber && c.Dimensions == "256x192")
                    .Select(c => c.PictureNumber)
                    .FirstOrDefault();

                product.SecondaryImageNumber = db.ProductPictures
                    .Where(c => c.ProductNumber == product.ProductNumber && c.Dimensions == "80x79")
                    .Select(c => c.PictureNumber)
                    .FirstOrDefault();
            }

            return product;
        }

        public static void SaveProduct(Product product)
        {
            Product productDb = new Product();
            ProductSupplier productSupplierDb = new ProductSupplier();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                productDb = db.Products.Find(product.ProductNumber);

                if (productDb != null)
                {
                    productDb.Activated = product.Activated;
                    productDb.DateModified = DateTime.Now;
                    productDb.DepartmentNumber = product.DepartmentNumber;
                    productDb.Description = product.Description;
                    productDb.Size = product.Size;
                    productDb.Weight = product.Weight;
                    db.Entry(productDb).State = EntityState.Modified;
                }

                productSupplierDb = db.ProductSuppliers.Find(product.ProductNumber, product.SupplierNumber);

                if (productSupplierDb != null)
                {
                    productSupplierDb.PricePerUnit = product.PricePerUnit;
                    db.Entry(productSupplierDb).State = EntityState.Modified;
                }

                db.SaveChanges();
            }
        }

        public static void SaveProductImage(int productNumber, HttpPostedFileBase image, string dimensions)
        {
            if (image != null)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {

                }
            }
        }

        public override string ToString()
        {
            string toReturn = "";

            if (ProductNumber != 0 && SupplierNumber != 0 && Description != null && PricePerUnit != 0)
            {
                toReturn += string.Format("\nProduct Number     : {0}", ProductNumber);
                toReturn += string.Format("\nSupplier Number    : {0}", SupplierNumber);
                toReturn += string.Format("\nDescription        : {0}", Description);
                toReturn += string.Format("\nPrice Per Unit     : {0}", PricePerUnit);
            }

            return toReturn;
        }
    }
    public class ProductMetaData
    {
        [Required]
        [DisplayName("Description")]
        public string Description { get; set; }

        [Required]
        [DisplayName("Size")]
        public string Size { get; set; }

        [Required]
        [DisplayName("Weight")]
        public decimal Weight { get; set; }

        [DisplayName("Product Number")]
        public int ProductNumber { get; set; }

        [DisplayName("Date Added")]
        public DateTime DateAdded { get; set; }

        [DisplayName("Date Modified")]
        public DateTime DateModified { get; set; }

        [DisplayName("Department Number")]
        public DateTime DepartmentNumber { get; set; }
    }
}