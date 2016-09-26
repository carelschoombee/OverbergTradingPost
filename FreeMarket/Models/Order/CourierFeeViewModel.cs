using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FreeMarket.Models
{
    public class CourierFeeViewModel
    {
        [DisplayName("Quantity")]
        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }

        public bool? CannotDeliver { get; set; }
        public int ProductNumber { get; set; }
        public int SupplierNumber { get; set; }
        public Product ProductInstance { get; set; }
        public Supplier SupplierInstance { get; set; }
        public int ReviewPageSize { get; set; }
        public int CustodianQuantityOnHand { get; set; }

        public void InitializeDefault()
        {
            Quantity = 0;
            ProductNumber = 0;
            SupplierNumber = 0;
        }

        public CourierFeeViewModel()
        {
            InitializeDefault();
        }

        public CourierFeeViewModel(int productNumber, int supplierNumber, int quantityRequested, int orderNumber = 0)
        {
            // Validate
            if (productNumber == 0 || supplierNumber == 0 || quantityRequested < 1)
                InitializeDefault();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                // Validate
                ProductSupplier productSupplierTemp = db.ProductSuppliers.Find(productNumber, supplierNumber);

                if (productSupplierTemp == null)
                {
                    InitializeDefault();
                    return;
                }

                Quantity = quantityRequested;
                ProductNumber = productNumber;
                SupplierNumber = supplierNumber;

                if (orderNumber != 0)
                    CannotDeliver = ShoppingCart.CalculateDeliverableStatus(productNumber, supplierNumber, quantityRequested, orderNumber);
                else
                    CannotDeliver = null;

                SetInstances(productNumber, supplierNumber);
                ReviewPageSize = 4;
            }
        }

        public void SetInstances(int productNumber, int supplierNumber)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                Product product = Product.GetProduct(productNumber, supplierNumber);
                Supplier supplier = db.Suppliers.Find(supplierNumber);

                if (product == null || supplier == null)
                    return;

                ProductInstance = product;
                SupplierInstance = supplier;
            }
        }
    }
}