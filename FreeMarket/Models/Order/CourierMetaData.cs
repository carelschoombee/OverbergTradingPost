using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;

namespace FreeMarket.Models
{
    [MetadataType(typeof(CourierMetaData))]
    public partial class Courier
    {
        public int CourierReviewsCount { get; set; }

        public static Courier GetCourier(int courierNumber)
        {
            Courier courier = new Courier();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                courier = db.Couriers.Find(courierNumber);
            }

            return courier;
        }

        public static List<TimeFreightCourierFeeReference> GetTimeFreightPrices()
        {
            List<TimeFreightCourierFeeReference> prices = new List<TimeFreightCourierFeeReference>();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                prices = db.TimeFreightCourierFeeReferences.ToList();
            }

            return prices;
        }

        public static void SaveCourier(Courier courier)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                db.Entry(courier).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
    }
    public class CourierMetaData
    {
        [DisplayName("Number")]
        public int CourierNumber { get; set; }

        [DisplayName("Name")]
        [StringLength(100)]
        [Required]
        public string CourierName { get; set; }

        [DisplayName("Main Contact")]
        [StringLength(100)]
        public string MainContactName { get; set; }

        [DisplayName("Telephone")]
        [StringLength(25)]
        public string MainContactTelephoneNumber { get; set; }

        [DisplayName("Cellphone")]
        [StringLength(25)]
        public string MainContactCellphoneNumber { get; set; }

        [DisplayName("Email")]
        [StringLength(100)]
        public string MainContactEmailAddress { get; set; }

        [DisplayName("Bank Name")]
        [StringLength(100)]
        public string BankingDetailsBankName { get; set; }

        [DisplayName("Bank Branch Name")]
        [StringLength(100)]
        public string BankingDetailsBranchName { get; set; }

        [DisplayName("Bank Branch Code")]
        [StringLength(25)]
        public string BankingDetailsBranchCode { get; set; }

        [DisplayName("Bank Account")]
        [StringLength(25)]
        public string BankingDetailsAccountNumber { get; set; }

        [DisplayName("Bank Account Type")]
        [StringLength(50)]
        public string BankingDetailsAccountType { get; set; }

        [DisplayName("Date Added To System")]
        public DateTime DateAdded { get; set; }

        [DisplayName("Trusted")]
        public bool TrustedCourier { get; set; }

        [DisplayName("Activated")]
        public bool Activated { get; set; }

        [DisplayName("User Id")]
        public string UserId { get; set; }

        [DisplayName("Location")]
        public string LocationNumber { get; set; }
    }
}