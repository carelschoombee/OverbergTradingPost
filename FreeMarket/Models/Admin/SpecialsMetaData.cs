using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FreeMarket.Models
{
    [MetadataType(typeof(SpecialsMetaData))]
    public partial class Special
    {
        public static Special GetSpecial(int specialID)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                Special model = new Special();

                model = db.Specials.Find(specialID);

                if (model == null)
                    model = new Special();

                return model;
            }
        }

        public static Special SaveModel(Special model)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                Special oldConfig = db.Specials.AsNoTracking()
                    .Where(c => c.SpecialID == model.SpecialID)
                    .FirstOrDefault();

                model.DateModified = DateTime.Now;
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return oldConfig;
            }
        }

        public static Special GetNewSpecial()
        {
            Special special = new Special();

            special.DateAdded = DateTime.Now;

            return special;
        }

        public static void CreateNewSpecial(Special special)
        {
            special.DateAdded = DateTime.Now;

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                db.Specials.Add(special);
                db.SaveChanges();
            }
        }
    }

    public class SpecialsMetaData
    {
        [DisplayName("ID")]
        public int SpecialID { get; set; }

        [Required]
        [DisplayName("Postal Code Range Start")]
        [StringLength(50)]
        public string SpecialPostalCodeRangeStart { get; set; }

        [Required]
        [DisplayName("Postal Code Range End")]
        [StringLength(50)]
        public string SpecialPostalCodeRangeEnd { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [DisplayName("Delivery Fee")]
        public decimal DeliveryFee { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

        [DisplayName("Date Added")]
        public DateTime DateAdded { get; set; }

        [DisplayName("Date Modified")]
        public DateTime DateModified { get; set; }
    }
}