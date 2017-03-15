using System;
using System.Collections.Generic;

namespace FreeMarket.Models
{
    public class ControlPanelViewModel
    {
        public List<WebsiteFunction> Functions { get; set; }
        public string SMSCredits { get; set; }

        public ControlPanelViewModel()
        {
            Functions = WebsiteFunction.GetAllFunctions();
        }

        public async void SetSMSCredits()
        {
            try
            {
                SMSHelper helper = new SMSHelper();
                SMSCredits = await helper.CheckCredits();
            }
            catch (Exception e)
            {
                ExceptionLogging.LogException(e);
            }
        }
    }
}