using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Web.Mvc;

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
            Debug.Write("CourierFeeViewModel::Initializing empty.");
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

                SetModel(productNumber, supplierNumber, courierNumber, quantityRequested, orderNumber);
            }
        }

        public void SetModel(int productNumber, int supplierNumber, int courierNumber, int quantityRequested, int orderNumber)
        {
            Quantity = quantityRequested;
            ProductNumber = productNumber;
            SupplierNumber = supplierNumber;

            ReviewPageSize = 4;

            SetInstances(productNumber, supplierNumber);

            FeeInfo = CourierFee.GetCourierFees(productNumber, supplierNumber, quantityRequested, orderNumber);

            Debug.Write(string.Format("Model::{0}", FeeInfo.ToString()));

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