using System.ComponentModel.DataAnnotations;

namespace FreeMarket.Models
{
    [MetadataType(typeof(OrderDetailMetaData))]
    public partial class OrderDetail
    {
        public string SupplierName { get; set; }
        public string CourierName { get; set; }
        public string ProductDescription { get; set; }
        public string ProductDepartment { get; set; }
        public decimal ProductWeight { get; set; }
        public decimal ProductPrice { get; set; }
        public int QuantityOnHand { get; set; }
        public int MainImageNumber { get; set; }
        public bool Selected { get; set; }

        public override string ToString()
        {
            string toString = "";

            if (ItemNumber != 0 && !string.IsNullOrEmpty(ProductDescription) && ProductPrice != 0 && Quantity != 0 && OrderItemValue != 0)
            {
                toString += string.Format(("\nItem Number: {0}"), ItemNumber);
                toString += string.Format(("\nDescription: {0}"), ProductDescription);
                toString += string.Format(("\nPrice: {0}"), ProductPrice);
                toString += string.Format(("\nQuantity: {0}"), Quantity);
                toString += string.Format(("\nOrder Item Value: {0}"), OrderItemValue);
            }

            return toString;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            var item = obj as OrderDetail;

            if (item == null || GetType() != item.GetType())
            {
                return false;
            }

            if (this.ProductNumber == item.ProductNumber && this.Quantity == item.Quantity
                && this.SupplierNumber == item.SupplierNumber && this.CourierNumber == item.CourierNumber)
                return true;
            else
                return false;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return ProductNumber.GetHashCode() ^ SupplierNumber.GetHashCode();
        }
    }
    public class OrderDetailMetaData
    {
    }
}