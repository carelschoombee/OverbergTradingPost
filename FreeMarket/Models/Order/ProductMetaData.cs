using System.ComponentModel.DataAnnotations;

namespace FreeMarket.Models
{
    [MetadataType(typeof(ProductMetaData))]
    public partial class Product
    {
        public int MainImageNumber { get; set; }
        public int SupplierNumber { get; set; }
        public string SupplierName { get; set; }
        public string DepartmentName { get; set; }
        public decimal PricePerUnit { get; set; }
        public int CustodianNumber { get; set; }
        public int QuantityOnHand { get; set; }

        public override string ToString()
        {
            string toReturn = "";

            if (ProductNumber != 0 && SupplierNumber != 0 && Description != null && PricePerUnit != 0)
            {
                toReturn += "\n---------------------";
                toReturn += string.Format("\nProduct Number: {0}", ProductNumber);
                toReturn += string.Format("\nSupplier Number: {0}", SupplierNumber);
                toReturn += string.Format("\nDescription: {0}", Description);
                toReturn += string.Format("\nPrice Per Unit: {0}", PricePerUnit);
                toReturn += "\n---------------------";
            }

            return toReturn;
        }
    }
    public class ProductMetaData
    {
    }
}