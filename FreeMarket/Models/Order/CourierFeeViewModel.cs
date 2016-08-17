using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace FreeMarket.Models
{
    public class CourierFeeViewModel
    {
        public List<CourierFee> FeeInfo { get; set; }
        public int Quantity { get; set; }

        public int SelectedAddress { get; set; }
        public List<SelectListItem> AddressNameOptions { get; set; }

        public CourierFeeViewModel()
        {
            FeeInfo = new List<CourierFee>();
            AddressNameOptions = new List<SelectListItem>();
            Quantity = 0;
            SelectedAddress = 0;
        }

        public CourierFeeViewModel(int productNumber, int supplierNumber, int quantityRequested, string userId, string defaultAddressName)
        {
            if (productNumber == 0 || supplierNumber == 0 || quantityRequested < 1 || string.IsNullOrEmpty(userId))
            {
                FeeInfo = new List<CourierFee>();
                AddressNameOptions = new List<SelectListItem>();
                Quantity = 0;
                SelectedAddress = 0;
            }
            else
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    AddressNameOptions = db.CustomerAddresses.Select
                        (c => new SelectListItem
                        {
                            Text = c.AddressName,
                            Value = c.AddressNumber.ToString(),
                            Selected = (c.AddressName == defaultAddressName ? true : false)
                        }).ToList();

                    SelectedAddress = int.Parse
                        (AddressNameOptions
                        .Where(c => c.Text == defaultAddressName)
                        .Select(c => c.Value)
                        .FirstOrDefault()
                        );
                }

                Quantity = quantityRequested;

                FeeInfo = CourierFee.GetCourierFees(productNumber, supplierNumber, quantityRequested, SelectedAddress);
            }
        }
    }
}