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
        public int CustodianNumber { get; set; }
        public int OrderNumber { get; set; }

        public bool FromCart { get; set; }

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

        public CourierFeeViewModel(int productNumber, int supplierNumber, int quantityRequested, string userId, string defaultAddressName, string addressString = null)
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

                if (!string.IsNullOrEmpty(addressString))
                {
                    // Get all addresses for the customer
                    List<CustomerAddress> allAddresses = db.CustomerAddresses
                        .Where(c => c.CustomerNumber == userId)
                        .ToList();

                    foreach (CustomerAddress add in allAddresses)
                    {
                        // Show the selected address first to the customer on the modal
                        if (add.ToString().Replace("\n", "") == addressString)
                        {
                            SelectedAddress = add.AddressNumber;
                            string selected = add.AddressName;

                            AddressNameOptions = allAddresses
                                .Select
                                (c => new SelectListItem
                                {
                                    Text = c.AddressName,
                                    Value = c.AddressNumber.ToString(),
                                    Selected = (c.AddressName == selected ? true : false)
                                }).ToList();
                        }
                    }
                }

                // If the user has not yet chosen an address on the modal display the default address
                if (SelectedAddress == 0)
                {
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

                SetModel(productNumber, supplierNumber, quantityRequested, userId);
            }
        }

        public CourierFeeViewModel(int productNumber, int supplierNumber, int quantityRequested, string userId, int addressNumber)
        {
            // Validate
            if (productNumber == 0 || supplierNumber == 0 || quantityRequested < 1 || string.IsNullOrEmpty(userId))
                InitializeDefault();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                ProductSupplier productSupplierTemp = db.ProductSuppliers.Find(productNumber, supplierNumber);

                if (productSupplierTemp == null)
                {
                    InitializeDefault();
                    return;
                }

                AddressNameOptions = db.CustomerAddresses
                    .Where(c => c.CustomerNumber == userId)
                    .Select
                    (c => new SelectListItem
                    {
                        Text = c.AddressName,
                        Value = c.AddressNumber.ToString(),
                        Selected = (c.AddressNumber == addressNumber ? true : false)
                    }).ToList();

                SelectedAddress = addressNumber;
            }

            SetModel(productNumber, supplierNumber, quantityRequested, userId);
        }

        public void SetModel(int productNumber, int supplierNumber, int quantityRequested, string userId)
        {
            Quantity = quantityRequested;
            ProductNumber = productNumber;
            SupplierNumber = supplierNumber;

            FeeInfo = CourierFee.GetCourierFees(productNumber, supplierNumber, quantityRequested, SelectedAddress);

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