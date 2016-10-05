using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FreeMarket.Models
{
    [MetadataType(typeof(SupplierMetaData))]
    public partial class Supplier
    {

    }
    public class SupplierMetaData
    {
        [DisplayName("Number")]
        public int SupplierNumber { get; set; }

        [DisplayName("Name")]
        [StringLength(100)]
        public string SupplierName { get; set; }

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
        public bool TrustedSupplier { get; set; }

        [DisplayName("Activated")]
        public bool Activated { get; set; }

        [DisplayName("User Id")]
        public string UserId { get; set; }
    }
}