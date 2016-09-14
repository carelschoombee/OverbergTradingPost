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
    
    public partial class ProductReview
    {
        public int ReviewId { get; set; }
        public Nullable<int> ProductNumber { get; set; }
        public Nullable<int> OrderNumber { get; set; }
        public string Author { get; set; }
        public string UserId { get; set; }
        public Nullable<short> StarRating { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string ReviewContent { get; set; }
        public Nullable<int> SupplierNumber { get; set; }
    
        public virtual Product Product { get; set; }
    }
}