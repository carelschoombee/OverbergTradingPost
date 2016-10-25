using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FreeMarket.Models
{
    [MetadataType(typeof(TimeFreightCourierFeeReferenceMetaData))]
    public partial class TimeFreightCourierFeeReference
    {
        public static void SaveModel(TimeFreightCourierFeeReference model)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }
    }

    public class TimeFreightCourierFeeReferenceMetaData
    {
        [DisplayName("ID")]
        public int DeliveryCostID { get; set; }

        [StringLength(10)]
        [DisplayName("Code")]
        public string Code { get; set; }

        [Required]
        [DisplayName("Postal Code Range Start")]
        public int PostalCodeRangeStart { get; set; }

        [Required]
        [DisplayName("Postal Code Range End")]
        public int PostalCodeRangeEnd { get; set; }

        [Required]
        [DisplayName("Postal Code Range Start")]
        public int WeightStartRange { get; set; }

        [Required]
        [DisplayName("Postal Code Range End")]
        public int WeightEndRange { get; set; }

        [StringLength(50)]
        [DisplayName("Main Centre")]
        public string MainCentre { get; set; }

        [StringLength(10)]
        [DisplayName("Dialing Code")]
        public string DialingCode { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [DisplayName("Delivery Fee")]
        public decimal DeliveryFee { get; set; }
    }
}