using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FreeMarket.Models
{
    [MetadataType(typeof(PriceHistoryMetaData))]
    public partial class PriceHistory
    {
        public string Description { get; set; }
        public string SupplierName { get; set; }

        public static List<PriceHistory> GetAllHistories()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                List<PriceHistory> result = db.GetPriceHistories()
                    .Select(c => new PriceHistory
                    {
                        Date = c.Date,
                        Description = c.Description,
                        NewPrice = c.NewPrice,
                        PriceID = c.PriceID,
                        OldPrice = c.OldPrice,
                        SupplierName = c.SupplierName,
                        SupplierNumber = c.SupplierNumber,
                        ProductNumber = c.ProductNumber,
                        Type = c.Type

                    }).ToList();

                if (result == null)
                    result = new List<PriceHistory>();

                return result;
            }
        }
    }

    public class PriceHistoryMetaData
    {
    }
}