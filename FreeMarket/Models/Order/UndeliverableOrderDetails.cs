using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FreeMarket.Models
{
    [MetadataType(typeof(UndeliverableOrderDetailsMetaData))]
    public partial class UndeliverableOrderDetail
    {
        public int MainImageNumber { get; set; }
        public string ProductDescription { get; set; }
        public string SupplierName { get; set; }
        public bool Selected { get; set; }

        public static List<UndeliverableOrderDetail> GetUndeliverables(int orderNumber)
        {
            if (orderNumber == 0)
                return new List<UndeliverableOrderDetail>();

            List<UndeliverableOrderDetail> collection = new List<UndeliverableOrderDetail>();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                collection = db.UndeliverableOrderDetails
                    .Where(c => c.OrderNumber == orderNumber)
                    .ToList();

                if (collection != null && collection.Count > 0)
                {
                    foreach (UndeliverableOrderDetail detail in collection)
                    {
                        int imageNumber = db.ProductPictures
                            .Where(c => c.ProductNumber == detail.ProductNumber && c.Dimensions == PictureSize.Small.ToString())
                            .Select(c => c.PictureNumber)
                            .FirstOrDefault();

                        detail.MainImageNumber = imageNumber;

                        string description = db.Products.Where(c => c.ProductNumber == detail.ProductNumber)
                            .Select(c => c.Description)
                            .FirstOrDefault();

                        detail.ProductDescription = description;

                        string supplierName = db.Suppliers.Where(c => c.SupplierNumber == detail.SupplierNumber)
                            .Select(c => c.SupplierName)
                            .FirstOrDefault();

                        detail.SupplierName = supplierName;
                    }
                }
            }

            return collection;
        }
    }

    public class UndeliverableOrderDetailsMetaData
    {
    }
}