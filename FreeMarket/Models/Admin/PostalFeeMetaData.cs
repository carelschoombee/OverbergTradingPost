using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FreeMarket.Models
{
    [MetadataType(typeof(PostalFeeMetaData))]
    public partial class PostalFee
    {
        public static PostalFee GetPostalFee(int Id)
        {
            PostalFee model = new PostalFee();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                model = db.PostalFees.Find(Id);
            }

            if (model == null)
                model = new PostalFee();

            return model;
        }

        public static PostalFee SaveModel(PostalFee model)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                PostalFee oldFee = db.PostalFees.AsNoTracking()
                    .Where(c => c.Id == model.Id)
                    .FirstOrDefault();

                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return oldFee;
            }
        }
    }

    public class PostalFeeMetaData
    {
        [DisplayName("ID")]
        public int Id { get; set; }

        [Required]
        [DisplayName("Weight")]
        public decimal Weight { get; set; }

        [Required]
        [DisplayName("Price")]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [DisplayName("Price per additional KG extra")]
        public decimal PerKgExtraPrice { get; set; }
    }
}