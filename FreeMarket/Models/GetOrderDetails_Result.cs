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
    
    public partial class GetOrderDetails_Result
    {
        public int ItemNumber { get; set; }
        public int OrderNumber { get; set; }
        public int SupplierNumber { get; set; }
        public int ProductNumber { get; set; }
        public Nullable<int> CustodianNumber { get; set; }
        public Nullable<bool> Settled { get; set; }
        public Nullable<bool> PaySupplier { get; set; }
        public Nullable<bool> PayCourier { get; set; }
        public Nullable<bool> PaidSupplier { get; set; }
        public Nullable<bool> PaidCourier { get; set; }
        public string OrderItemStatus { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal OrderItemValue { get; set; }
        public string Description { get; set; }
        public string SupplierName { get; set; }
        public decimal Weight { get; set; }
        public bool IsVirtual { get; set; }
    }
}
