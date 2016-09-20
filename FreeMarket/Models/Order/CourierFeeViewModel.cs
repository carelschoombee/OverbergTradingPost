using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace FreeMarket.Models
{
    public class CourierFeeViewModel
    {
        public List<CourierFee> FeeInfo { get; set; }

        [DisplayName("Quantity")]
        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }
        public int SelectedCourierNumber { get; set; }

        public int ProductNumber { get; set; }
        public int SupplierNumber { get; set; }
        public int CustodianNumber { get; set; }
        public int OrderNumber { get; set; }

        public Product ProductInstance { get; set; }
        public Supplier SupplierInstance { get; set; }

        public int ReviewPageSize { get; set; }

        public bool FromCart { get; set; }

        public void InitializeDefault()
        {
            FeeInfo = new List<CourierFee>();
            Quantity = 0;
            SelectedCourierNumber = 0;
            ProductNumber = 0;
            SupplierNumber = 0;
        }

        public CourierFeeViewModel()
        {
            InitializeDefault();
        }

        //Anonymous User
        public CourierFeeViewModel(int productNumber, int supplierNumber, int quantityRequested)
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

                ReviewPageSize = 4;

                SetInstances(productNumber, supplierNumber);

                FeeInfo = new List<CourierFee>();

                Debug.Write(string.Format("Model::{0}", FeeInfo.ToString()));

                SelectedCourierNumber = 0;
            }
        }

        // Logged in user
        public CourierFeeViewModel(int productNumber, int supplierNumber, int courierNumber, int quantityRequested, int orderNumber, string userId)
        {
            // Validate
            if (productNumber == 0 || supplierNumber == 0 || quantityRequested < 1 || string.IsNullOrEmpty(userId))
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

                ReviewPageSize = 4;

                SetInstances(productNumber, supplierNumber);

                FeeInfo = CourierFee.GetCourierFees(productNumber, supplierNumber, quantityRequested, orderNumber);

                if (courierNumber == 0)
                {
                    if (FeeInfo == null || FeeInfo.Count == 0)
                        SelectedCourierNumber = 0;
                    else
                        SelectedCourierNumber = FeeInfo[0].CourierNumber;
                }
                else
                {
                    SelectedCourierNumber = courierNumber;
                }
            }
        }

        public static CourierFeeViewModel GetCourierDataDoWork(int id, int supplierNumber, int quantity, ShoppingCart cart, string userId)
        {
            CourierFeeViewModel model = new CourierFeeViewModel();
            bool anonymousUser = (userId == null);

            // Validate
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                Product product = db.Products.Find(id);
                Supplier supplier = db.Suppliers.Find(supplierNumber);

                if (product == null || supplier == null)
                    return null;
            }

            if (anonymousUser)
            {
                model = new CourierFeeViewModel(id, supplierNumber, quantity);
                return model;
            }
            else
            {
                // If this item is already in the basket, assign the courier used for this item in the ui message.
                OrderDetail detail = cart.GetOrderDetail(id, supplierNumber);
                int courierNumber = 0;

                if (detail == null)
                    courierNumber = 0;
                else
                    courierNumber = (int)detail.CourierNumber;

                model = new CourierFeeViewModel(id, supplierNumber, courierNumber, quantity, cart.Order.OrderNumber, userId);

                // Determine no charge
                foreach (OrderDetail temp in cart.Body.OrderDetails)
                {
                    foreach (CourierFee info in model.FeeInfo)
                    {
                        using (FreeMarketEntities db = new FreeMarketEntities())
                        {
                            if (info.CustodianNumber == temp.CustodianNumber &&
                                info.CourierNumber == temp.CourierNumber)
                            {
                                info.NoCharge = true;
                            }
                        }
                    }
                }
            }

            return model;
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

        public override string ToString()
        {
            string toString = "";

            toString += "\nCourierFeeViewModel::ToString()\n";

            if (FeeInfo != null && FeeInfo.Count > 0)
            {
                foreach (CourierFee detail in FeeInfo)
                {
                    toString += string.Format("\n{0}\n", detail.ToString());
                }
            }

            toString += "\nCourierFeeViewModel::ToString()\n";
            toString += string.Format("Total Items in Cart: {0}", FeeInfo.Count);

            return toString;
        }
    }
}