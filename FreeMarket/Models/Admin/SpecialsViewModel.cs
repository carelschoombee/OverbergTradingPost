using System.Collections.Generic;
using System.Linq;

namespace FreeMarket.Models
{
    public class SpecialsViewModel
    {
        public List<Special> Specials { get; set; }

        public static SpecialsViewModel GetModel()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                SpecialsViewModel model = new SpecialsViewModel();

                model.Specials = new List<Special>();

                model.Specials = db.Specials.ToList();

                if (model == null)
                    model = new SpecialsViewModel();

                return model;
            }
        }
    }
}