using System.Collections.Generic;
using System.Linq;

namespace FreeMarket.Models
{
    public class PostOfficeViewModel
    {
        public List<PostalFee> PostOfficeFees { get; set; }

        public static PostOfficeViewModel GetModel()
        {
            PostOfficeViewModel model = new PostOfficeViewModel();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                model.PostOfficeFees = db.PostalFees.ToList();
            }

            return model;
        }
    }
}