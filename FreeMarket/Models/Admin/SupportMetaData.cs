using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FreeMarket.Models
{
    [MetadataType(typeof(SupportMetaData))]
    public partial class Support
    {
        public ProductCollection ActivatedProducts { get; set; }

        public static Support GetSupport()
        {
            Support support = new Support();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                support = db.Supports.FirstOrDefault();

                if (support == null)
                    return new Support();

            }

            return support;
        }

        public static void SaveModel(Support support)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                db.Entry(support).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }
    }

    public class SupportMetaData
    {
        [Required]
        [StringLength(50)]
        [DisplayName("Land Line")]
        public string Landline { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Cellphone")]
        public string Cellphone { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Email for Orders")]
        public string OrdersEmail { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Email for Information")]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Main Contact Name")]
        public string MainContactName { get; set; }

        [Required]
        [StringLength(200)]
        [DisplayName("Street Address")]
        public string StreetAddress { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Province")]
        public string Province { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Postal Code")]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(200)]
        [DisplayName("Town Name")]
        public string TownName { get; set; }
    }
}