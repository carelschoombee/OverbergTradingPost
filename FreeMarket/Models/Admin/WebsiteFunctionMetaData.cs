using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FreeMarket.Models
{
    [MetadataType(typeof(WebsiteFunctionMetaData))]
    public partial class WebsiteFunction
    {
        public static List<WebsiteFunction> GetAllFunctions()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                return db.WebsiteFunctions.ToList();
            }
        }
    }

    public class WebsiteFunctionMetaData
    {

    }
}