using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace FreeMarket.Models
{
    public class AspNetUserCustomer
    {
        public AspNetUser User { get; set; }
        public List<SelectListItem> CommunicationOptions { get; set; }
        public string SelectedCommunicationOption { get; set; }
        public bool DisplayOnly { get; set; }

        public AspNetUserCustomer()
        {

        }

        public AspNetUserCustomer(string customerNumber, bool displayOnly)
        {
            User = new AspNetUser();
            CommunicationOptions = new List<SelectListItem>();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                User = db.AspNetUsers.Find(customerNumber);

                if (User == null)
                    User = new AspNetUser();

                CommunicationOptions = db.PreferredCommunicationMethods.Select
                    (c => new SelectListItem
                    {
                        Text = c.CommunicationMethod,
                        Value = c.CommunicationMethod
                    }).ToList();

                SelectedCommunicationOption = User.PreferredCommunicationMethod;
                DisplayOnly = displayOnly;
            }
        }
    }
}