using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace FreeMarket.Models
{
    public class SaveCartViewModel
    {
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> prefDeliveryDateTime { get; set; }

        [DisplayName("Delivery Address")]
        public int SelectedAddress { get; set; }

        public List<SelectListItem> AddressNameOptions { get; set; }

        public string AddressName { get; set; }
        public CustomerAddress Address { get; set; }

        public SaveCartViewModel() { }

        public SaveCartViewModel(string customerNumber, OrderHeader order)
        {
            SetModel(customerNumber, order);
        }

        public void SetModel(string customerNumber, OrderHeader order)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                List<CustomerAddress> addresses = db.CustomerAddresses
                    .Where(c => c.CustomerNumber == customerNumber)
                    .ToList();

                CustomerAddress address = addresses
                    .Where(c => c.ToString() == order.DeliveryAddress)
                    .FirstOrDefault();

                int selectedAddressNumber = 0;
                string addressName = "";

                if (address == null)
                {
                    selectedAddressNumber = 0;
                    addressName = "Current";
                }
                else
                {
                    selectedAddressNumber = address.AddressNumber;
                    addressName = address.AddressName;
                }

                SetAddressNameOptions(customerNumber, selectedAddressNumber);

                if (address == null)
                {
                    AddressNameOptions.Add(new SelectListItem { Text = "Current", Value = "0", Selected = true });
                    Address = new CustomerAddress
                    {
                        AddressCity = order.DeliveryAddressCity,
                        AddressLine1 = order.DeliveryAddressLine1,
                        AddressLine2 = order.DeliveryAddressLine2,
                        AddressLine3 = order.DeliveryAddressLine3,
                        AddressLine4 = order.DeliveryAddressLine3,
                        AddressName = "Current",
                        AddressNumber = 0,
                        AddressPostalCode = order.DeliveryAddressPostalCode,
                        AddressSuburb = order.DeliveryAddressSuburb,
                        CustomerNumber = customerNumber
                    };
                }
                else
                {
                    Address = address;
                }

                if (order.DeliveryDate == null)
                {
                    // Create a suggested date
                    int i = 2;

                    while (DateTime.Now.AddDays(i).DayOfWeek.ToString() == "Saturday" || DateTime.Now.AddDays(i).DayOfWeek.ToString() == "Sunday")
                    {
                        ++i;
                    }

                    DateTime defaultDateTime = new DateTime(DateTime.Now.AddDays(i).Year, DateTime.Now.AddDays(i).Month, DateTime.Now.AddDays(i).Day, 12, 0, 0, DateTimeKind.Utc);
                    prefDeliveryDateTime = defaultDateTime;
                }
                else
                {
                    prefDeliveryDateTime = order.DeliveryDate;
                }

                AddressName = AddressNameOptions
                    .Where(c => c.Selected == true)
                    .Select(c => c.Text)
                    .FirstOrDefault();

                SelectedAddress = selectedAddressNumber;
            }
        }

        public void SetAddressNameOptions(string customerNumber, int selectedAddressNumber)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                AddressNameOptions = db.CustomerAddresses
                       .Where(c => c.CustomerNumber == customerNumber)
                       .Select
                       (c => new SelectListItem
                       {
                           Text = c.AddressName,
                           Value = c.AddressNumber.ToString(),
                           Selected = (c.AddressNumber == selectedAddressNumber ? true : false)
                       }).ToList();

                AddressName = AddressNameOptions
                   .Where(c => c.Selected == true)
                   .Select(c => c.Text)
                   .FirstOrDefault();
            }
        }
    }
}