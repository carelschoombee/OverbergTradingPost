//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FreeMarket.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Courier
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Courier()
        {
            this.CourierStockMovementLogs = new HashSet<CourierStockMovementLog>();
            this.CourierReviews = new HashSet<CourierReview>();
        }
    
        public int CourierNumber { get; set; }
        public string CourierName { get; set; }
        public string MainContactName { get; set; }
        public string MainContactTelephoneNumber { get; set; }
        public string MainContactCellphoneNumber { get; set; }
        public string MainContactEmailAddress { get; set; }
        public string BankingDetailsBankName { get; set; }
        public string BankingDetailsBranchName { get; set; }
        public string BankingDetailsBranchCode { get; set; }
        public string BankingDetailsAccountNumber { get; set; }
        public string BankingDetailsAccountType { get; set; }
        public Nullable<System.DateTime> DateAdded { get; set; }
        public Nullable<bool> TrustedCourier { get; set; }
        public Nullable<bool> Activated { get; set; }
        public string UserId { get; set; }
        public Nullable<int> LocationNumber { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CourierStockMovementLog> CourierStockMovementLogs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CourierReview> CourierReviews { get; set; }
    }
}
