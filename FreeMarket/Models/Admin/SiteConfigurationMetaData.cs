using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace FreeMarket.Models
{
    [MetadataType(typeof(SiteConfigurationMetaData))]
    public partial class SiteConfiguration
    {
        public static List<SiteConfiguration> GetSiteConfig()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                return db.SiteConfigurations.OrderBy(c => c.Key).ToList();
            }
        }

        public static SiteConfiguration GetSpecificSiteConfig(int id)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                return db.SiteConfigurations.Find(id);
            }
        }

        public static SiteConfiguration SaveConfig(SiteConfiguration config)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                SiteConfiguration oldConfig = db.SiteConfigurations.AsNoTracking()
                    .Where(c => c.Key == config.Key)
                    .FirstOrDefault();

                db.Entry(config).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return oldConfig;
            }
        }
    }

    public class SiteConfigurationMetaData
    {
        [AllowHtml]
        [Required]
        [DisplayName("Value")]
        public string Value { get; set; }

        [DisplayName("Key")]
        public string Key { get; set; }

        [DisplayName("ID")]
        public int SiteConfigurationNumber { get; set; }
    }
}