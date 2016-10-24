using System;
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

        public static List<AspNetUser> Filter(string searchCriteria)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                List<AspNetUser> allCustomers = db.AspNetUsers.ToList();
                List<AspNetUser> filteredCustomers = new List<AspNetUser>();

                if (string.IsNullOrEmpty(searchCriteria))
                    return allCustomers;

                if (allCustomers != null)
                {
                    filteredCustomers = db.FilterCustomers(searchCriteria).Select(c => new AspNetUser
                    {
                        AccessFailedCount = c.AccessFailedCount ?? 0,
                        DefaultAddress = c.DefaultAddress,
                        UnsubscribeFromAllCorrespondence = c.UnsubscribeFromAllCorrespondence ?? false,
                        Email = c.Email,
                        EmailConfirmed = c.EmailConfirmed ?? false,
                        PhoneNumberConfirmed = c.PhoneNumberConfirmed ?? false,
                        PreferredCommunicationMethod = c.PreferredCommunicationMethod,
                        UnConfirmedEmail = c.UnConfirmedEmail,
                        SecondaryPhoneNumber = c.SecondaryPhoneNumber,
                        SecurityStamp = c.SecurityStamp,
                        Id = c.Id,
                        LastVisited = c.LastVisited ?? DateTime.MinValue,
                        LockoutEnabled = c.LockoutEnabled ?? false,
                        LockoutEndDateUtc = c.LockoutEndDateUtc,
                        Name = c.Name,
                        PasswordHash = c.PasswordHash,
                        PhoneNumber = c.PhoneNumber,
                        TwoFactorEnabled = c.TwoFactorEnabled ?? false,
                        UnsubscribeFromRatings = c.UnsubscribeFromRatings ?? false,
                        UserName = c.UserName
                    }).ToList();

                    if (filteredCustomers != null && filteredCustomers.Count > 0)
                    {
                        if (filteredCustomers.Count == 1)
                        {
                            if (filteredCustomers.First().Id == null)
                            {
                                return new List<AspNetUser>();
                            }
                        }
                        return filteredCustomers;
                    }
                    else
                        return new List<AspNetUser>();

                }
                else
                {
                    allCustomers = new List<AspNetUser>();
                }

                return allCustomers;
            }
        }
    }
}