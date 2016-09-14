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
        public Nullable<System.DateTime> PrefDeliveryDateTime { get; set; }

        [DisplayName("Delivery Address")]
        public int SelectedAddress { get; set; }

        public List<SelectListItem> AddressNameOptions { get; set; }

        public SaveCartViewModel(string customerNumber, int addressNumber, DateTime deliveryDateTime)
        {
            SetModel(customerNumber, addressNumber, deliveryDateTime);
        }

        public void SetModel(string customerNumber, int addressNumber, DateTime deliveryDateTime)
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
                        Selected = (c.AddressNumber == addressNumber ? true : false)
                    }).ToList();

                SelectedAddress = addressNumber;

                PrefDeliveryDateTime = deliveryDateTime;
            }
        }
    }
}