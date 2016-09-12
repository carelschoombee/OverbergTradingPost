using System;
using System.ComponentModel.DataAnnotations;

namespace FreeMarket.Models
{
    public class SaveCartViewModel
    {
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> PrefDeliveryDateTime { get; set; }
    }
}