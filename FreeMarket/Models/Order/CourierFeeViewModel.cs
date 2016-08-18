using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
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

        [DisplayName("Delivery Address")]
        public int SelectedAddress { get; set; }

        public List<SelectListItem> AddressNameOptions { get; set; }
        public int SelectedCourierNumber { get; set; }

        public int ProductNumber { get; set; }
        public int SupplierNumber { get; set; }

        public void InitializeDefault()
        {
            FeeInfo = new List<CourierFee>();
            AddressNameOptions = new List<SelectListItem>();
            Quantity = 0;
            SelectedAddress = 0;
            SelectedCourierNumber = 0;
            ProductNumber = 0;
            SupplierNumber = 0;
        }

        public CourierFeeViewModel()
        {
            Debug.Write("CourierFeeViewModel::Initializing empty.");
            InitializeDefault();
        }

        public CourierFeeViewModel(int productNumber, int supplierNumber, int quantityRequested, string userId, string defaultAddressName)
        {
            if (productNumber == 0 || supplierNumber == 0 || quantityRequested < 1 || string.IsNullOrEmpty(userId))
            {
                InitializeDefault();
            }
            else
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    ProductSupplier productSupplierTemp = db.ProductSuppliers.Find(productNumber, supplierNumber);

                    if (productSupplierTemp == null)
                    {
                        InitializeDefault();
                        return;
                    }

                    ProductNumber = productNumber;
                    SupplierNumber = supplierNumber;

                    AddressNameOptions = db.CustomerAddresses
                        .Where(c => c.CustomerNumber == userId)
                        .Select
                        (c => new SelectListItem
                        {
                            Text = c.AddressName,
                            Value = c.AddressNumber.ToString(),
                            Selected = (c.AddressName == defaultAddressName ? true : false)
                        }).ToList();

                    SelectedAddress = db.CustomerAddresses
                        .Where(c => c.CustomerNumber == userId && c.AddressName == defaultAddressName)
                        .Select(c => c.AddressNumber)
                        .FirstOrDefault();
                }

                Quantity = quantityRequested;

                FeeInfo = CourierFee.GetCourierFees(productNumber, supplierNumber, quantityRequested, SelectedAddress);

                Debug.Write(string.Format("Model::{0}", FeeInfo.ToString()));

                if (FeeInfo == null || FeeInfo.Count == 0)
                    SelectedCourierNumber = 0;
                else
                    SelectedCourierNumber = FeeInfo[0].CourierNumber;
            }
        }

        public void UpdateModel(int productNumber, int supplierNumber, int quantityRequested, int selectedAddress)
        {
            if (productNumber == 0 || supplierNumber == 0 || quantityRequested == 0 || selectedAddress == 0)
                return;

            Quantity = quantityRequested;
            SelectedAddress = selectedAddress;
            ProductNumber = productNumber;
            SupplierNumber = supplierNumber;

            Debug.Write("CourierFeeViewModel::Updating model.");

            FeeInfo = CourierFee.GetCourierFees(productNumber, supplierNumber, quantityRequested, selectedAddress);

            Debug.Write(string.Format("Model::{0}", FeeInfo.ToString()));

            if (FeeInfo == null || FeeInfo.Count == 0)
                SelectedCourierNumber = 0;
            else
                SelectedCourierNumber = FeeInfo[0].CourierNumber;
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