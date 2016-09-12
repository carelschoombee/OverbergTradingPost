using System;
using System.ComponentModel.DataAnnotations;

namespace FreeMarket.Models
{
    public class SaveCartViewModel
    {
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> PrefDeliveryDateTime { get; set; }
    }
}