using FreeMarket.FreeMarketDataSetTableAdapters;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FreeMarket.Models
{
    [MetadataType(typeof(OrderHeaderMetaData))]
    public partial class OrderHeader
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPrimaryContactPhone { get; set; }
        public string CustomerPreferredCommunicationMethod { get; set; }
        public bool Selected { get; set; }

        public static OrderHeader GetOrderForShoppingCart(string customerNumber)
        {
            OrderHeader order = new OrderHeader();
            CustomerAddress address = new CustomerAddress();

            // Find the customer's details
            var UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = UserManager.FindById(customerNumber);

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                // Determine if the customer has an unconfirmed order
                order = db.OrderHeaders.Where(c => c.CustomerNumber == customerNumber
                                                    && (c.OrderStatus == "Unconfirmed"
                                                    || c.OrderStatus == "Locked")).FirstOrDefault();

                address = db.CustomerAddresses
                    .Where(c => c.CustomerNumber == customerNumber && c.AddressName == user.DefaultAddress)
                    .FirstOrDefault();

                if (address == null)
                {
                    address = new CustomerAddress();
                }

                // The customer has no unconfirmed orders
                if (order == null)
                {
                    order = new OrderHeader()
                    {
                        CustomerNumber = customerNumber,
                        OrderDatePlaced = DateTime.Now,
                        OrderDateClosed = null,
                        OrderStatus = "Unconfirmed",
                        PaymentReceived = false,
                        TotalOrderValue = 0,
                        CourierNumber = 1,

                        DeliveryAddress = address.ToString(),
                        DeliveryAddressLine1 = address.AddressLine1,
                        DeliveryAddressLine2 = address.AddressLine2,
                        DeliveryAddressLine3 = address.AddressLine3,
                        DeliveryAddressLine4 = address.AddressLine4,
                        DeliveryAddressSuburb = address.AddressSuburb,
                        DeliveryAddressCity = address.AddressCity,
                        DeliveryAddressPostalCode = address.AddressPostalCode,
                        DeliveryDate = null,
                        DeliveryDateAgreed = null,
                        DeliveryType = "Courier",

                        CustomerName = user.Name,
                        CustomerEmail = user.Email,
                        CustomerPrimaryContactPhone = user.PhoneNumber,
                        CustomerPreferredCommunicationMethod = user.PreferredCommunicationMethod
                    };

                    db.OrderHeaders.Add(order);
                    db.SaveChanges();

                    Debug.Write(string.Format("New order for shopping cart: {0}", order.ToString()));
                }
                // Set the customer details on the currently unconfirmed order
                else
                {
                    order.CustomerName = user.Name;
                    order.CustomerEmail = user.Email;
                    order.CustomerPrimaryContactPhone = user.PhoneNumber;
                    order.CustomerPreferredCommunicationMethod = user.PreferredCommunicationMethod;

                    Debug.Write(string.Format("Existing order for shopping cart: {0}", order.ToString()));
                }
            }

            // Return an order which can be used in a shopping cart
            return order;
        }

        public void UpdateDeliveryDetails(SaveCartViewModel model)
        {
            DeliveryDate = model.prefDeliveryDateTime;

            DeliveryAddress = model.Address.ToString();
            DeliveryAddressCity = model.Address.AddressCity;
            DeliveryAddressSuburb = model.Address.AddressSuburb;
            DeliveryAddressPostalCode = model.Address.AddressPostalCode;
            DeliveryAddressLine1 = model.Address.AddressLine1;
            DeliveryAddressLine2 = model.Address.AddressLine2;
            DeliveryAddressLine3 = model.Address.AddressLine3;
            DeliveryAddressLine4 = model.Address.AddressLine4;
            DeliveryType = model.DeliveryOptions.SelectedDeliveryType;

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                db.Entry(this).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static Dictionary<MemoryStream, string> GetReport(string reportType, int orderNumber)
        {
            MemoryStream stream = new MemoryStream();
            Dictionary<MemoryStream, string> outCollection = new Dictionary<MemoryStream, string>();

            try
            {
                GetOrderReportTableAdapter ta = new GetOrderReportTableAdapter();
                FreeMarketDataSet ds = new FreeMarketDataSet();

                ds.GetOrderReport.Clear();
                ds.EnforceConstraints = false;

                ta.Fill(ds.GetOrderReport, orderNumber);

                ReportDataSource rds = new ReportDataSource();
                rds.Name = "DataSet1";
                rds.Value = ds.GetOrderReport;

                ReportViewer rv = new Microsoft.Reporting.WebForms.ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;

                switch (reportType)
                {
                    case "Refund":
                        rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report6.rdlc");
                        break;
                    case "PostalInstructions":
                        rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report5.rdlc");
                        break;
                    case "PostalConfirmation":
                        rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report4.rdlc");
                        break;
                    case "StruisbaaiOrderConfirmation":
                        rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report3.rdlc");
                        break;
                    case "DeliveryInstructions":
                        rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report2.rdlc");
                        break;
                    case "OrderConfirmation":
                        rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report1.rdlc");
                        break;
                    default:
                        return new Dictionary<MemoryStream, string>();

                }

                rv.LocalReport.DataSources.Add(rds);
                rv.LocalReport.EnableHyperlinks = true;
                rv.LocalReport.Refresh();

                byte[] streamBytes = null;
                string mimeType = "";
                string encoding = "";
                string filenameExtension = "";
                string[] streamids = null;
                Warning[] warnings = null;

                streamBytes = rv.LocalReport.Render("PDF", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);

                stream = new MemoryStream(streamBytes);

                outCollection.Add(stream, mimeType);
            }
            catch (Exception e)
            {

            }

            return outCollection;
        }

        public async static void SendRatingEmail(string customerNumber, int orderNumber)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                ApplicationUser user = System.Web.HttpContext
                            .Current
                            .GetOwinContext()
                            .GetUserManager<ApplicationUserManager>()
                            .FindById(customerNumber);

                var requestContext = HttpContext.Current.Request.RequestContext;
                string url = "https://www.schoombeeandson.co.za" + new UrlHelper(requestContext).Action("RateOrder", "Manage", new { orderNumber = orderNumber });

                EmailService email = new EmailService();

                IdentityMessage iMessage = new IdentityMessage();
                iMessage.Destination = user.Email;

                string message1 = CreateRatingMessageCustomer();

                iMessage.Body = string.Format((message1), user.Name, url);
                iMessage.Subject = string.Format("Schoombee and Son Order");

                await email.SendAsync(iMessage);

                SMSHelper helper = new SMSHelper();

                string smsLine1 = db.SiteConfigurations
                    .Where(c => c.Key == "OrderRateSmsLine1")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                await helper.SendMessage(string.Format(smsLine1, user.Name, orderNumber, url), user.PhoneNumber);
            }
        }

        private static string CreateRatingMessageCustomer()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                string line1 = db.SiteConfigurations
                   .Where(c => c.Key == "OrderRateLine1")
                   .Select(c => c.Value)
                   .FirstOrDefault();

                string line2 = db.SiteConfigurations
                    .Where(c => c.Key == "OrderRateLine2")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                string line3 = db.SiteConfigurations
                    .Where(c => c.Key == "OrderRateLine3")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                string line4 = db.SiteConfigurations
                    .Where(c => c.Key == "OrderRateLine4")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                string line5 = db.SiteConfigurations
                    .Where(c => c.Key == "OrderRateLine5")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                string line6 = db.SiteConfigurations
                    .Where(c => c.Key == "OrderRateLine6")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                return line1 + line2 + line3 + line4 + line5 + line6;
            }
        }

        public async static void SendRefundEmail(string customerNumber, int orderNumber)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                Dictionary<MemoryStream, string> refundSummary;

                OrderHeader oh = db.OrderHeaders.Find(orderNumber);

                if (oh != null)
                {
                    string message1 = CreateRefundMessageCustomer();

                    Support supportInfo = db.Supports
                        .FirstOrDefault();

                    refundSummary = GetReport(ReportType.Refund.ToString(), orderNumber);

                    ApplicationUser user = System.Web.HttpContext
                                .Current
                                .GetOwinContext()
                                .GetUserManager<ApplicationUserManager>()
                                .FindById(customerNumber);

                    EmailService email = new EmailService();

                    IdentityMessage iMessage = new IdentityMessage();
                    iMessage.Destination = user.Email;

                    iMessage.Body = string.Format((message1), user.Name, orderNumber, supportInfo.Cellphone, supportInfo.Landline, supportInfo.Email);
                    iMessage.Subject = string.Format("Schoombee and Son Refund");

                    await email.SendAsync(iMessage, refundSummary.FirstOrDefault().Key);

                    IdentityMessage iMessageNotifyRefund = new IdentityMessage();
                    iMessageNotifyRefund.Destination = supportInfo.OrdersEmail;

                    string usLine1 = db.SiteConfigurations
                        .Where(c => c.Key == "OrderRefundUsLine1")
                        .Select(c => c.Value)
                        .FirstOrDefault();

                    iMessageNotifyRefund.Body = string.Format((usLine1), orderNumber, user.Name, user.Email, user.PhoneNumber, user.SecondaryPhoneNumber);
                    iMessageNotifyRefund.Subject = string.Format("Refund - Order {0}", orderNumber);

                    await email.SendAsync(iMessageNotifyRefund, refundSummary.FirstOrDefault().Key);

                    SMSHelper helper = new SMSHelper();

                    string smsLine1 = db.SiteConfigurations
                        .Where(c => c.Key == "OrderRefundSmsLine1")
                        .Select(c => c.Value)
                        .FirstOrDefault();

                    await helper.SendMessage(string.Format(smsLine1, user.Name, orderNumber), user.PhoneNumber);
                }
            }
        }

        private static string CreateRefundMessageCustomer()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                string line1 = db.SiteConfigurations
                    .Where(c => c.Key == "OrderRefundLine1")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                string line2 = db.SiteConfigurations
                    .Where(c => c.Key == "OrderRefundLine2")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                string line3 = db.SiteConfigurations
                    .Where(c => c.Key == "OrderRefundLine3")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                string line4 = db.SiteConfigurations
                    .Where(c => c.Key == "OrderRefundLine4")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                return line1 + line2 + line3 + line4;
            }
        }

        public async static void SendConfirmationEmail(string customerNumber, int orderNumber)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                ApplicationUser user = System.Web.HttpContext
                            .Current
                            .GetOwinContext()
                            .GetUserManager<ApplicationUserManager>()
                            .FindById(customerNumber);

                Dictionary<MemoryStream, string> orderSummary;
                Dictionary<MemoryStream, string> orderDeliveryInstruction;

                OrderHeader order = db.OrderHeaders.Find(orderNumber);

                Support supportInfo = db.Supports
                    .FirstOrDefault();

                bool specialDelivery = false;

                int postalCode = 0;

                try
                {
                    postalCode = int.Parse(order.DeliveryAddressPostalCode);
                }
                catch (Exception e)
                {

                }

                if (db.ValidateSpecialDeliveryCode(postalCode).First() == 1)
                {
                    orderSummary = GetReport(ReportType.StruisbaaiOrderConfirmation.ToString(), orderNumber);
                    orderDeliveryInstruction = GetReport(ReportType.StruisbaaiOrderConfirmation.ToString(), orderNumber);
                    specialDelivery = true;
                }
                else
                {
                    if (order.DeliveryType == "Courier")
                    {
                        orderSummary = GetReport(ReportType.OrderConfirmation.ToString(), orderNumber);
                        orderDeliveryInstruction = GetReport(ReportType.DeliveryInstructions.ToString(), orderNumber);
                    }
                    else
                    {
                        orderSummary = GetReport(ReportType.PostalConfirmation.ToString(), orderNumber);
                        orderDeliveryInstruction = GetReport(ReportType.PostalInstructions.ToString(), orderNumber);
                    }
                }

                EmailService email = new EmailService();

                IdentityMessage iMessage = new IdentityMessage();
                iMessage.Destination = user.Email;

                string message1 = CreateConfirmationMessageCustomer();

                iMessage.Body = string.Format((message1), user.Name, supportInfo.MainContactName, supportInfo.Landline, supportInfo.Cellphone, supportInfo.Email);
                iMessage.Subject = string.Format("Schoombee and Son Order");

                await email.SendAsync(iMessage, orderSummary.FirstOrDefault().Key);

                SMSHelper helper = new SMSHelper();

                DateTime dateDispatch = GetDispatchDay((DateTime)order.DeliveryDate);
                DateTime dateArrive = GetArriveDay((DateTime)order.DeliveryDate);

                if (ConfigurationManager.AppSettings["sendSMSToSupportOnOrderConfirmed"] == "true")
                {
                    await helper.SendMessage(string.Format("Customer: {0}, has placed order {1}. Date of dispatch: {2}. Order Total: {3}. Delivery mechanism: {4}."
                                           , user.Name, order.OrderNumber, string.Format("{0:f}", dateDispatch), string.Format("{0:C}", order.TotalOrderValue),
                                           order.DeliveryType), string.Format("{0},{1}", supportInfo.Cellphone, supportInfo.Landline));

                }

                if (order.DeliveryType == "Courier")
                {
                    string smsLine1 = db.SiteConfigurations
                        .Where(c => c.Key == "OrderConfirmationSmsLine1")
                        .Select(c => c.Value)
                        .FirstOrDefault();

                    await helper.SendMessage(string.Format(smsLine1, user.Name, order.OrderNumber,
                        string.Format("{0:d}", dateDispatch), string.Format("{0:f}", dateArrive))
                        , user.PhoneNumber);

                    Courier courier = db.Couriers.Find(1);

                    if (courier != null)
                    {
                        string message2 = CreateCourierInstructionsMessage();

                        IdentityMessage iMessageCourier = new IdentityMessage();

                        if (specialDelivery)
                            iMessageCourier.Destination = supportInfo.OrdersEmail;
                        else
                            iMessageCourier.Destination = courier.MainContactEmailAddress;

                        iMessageCourier.Body = string.Format((message2), orderNumber, supportInfo.MainContactName, supportInfo.Landline, supportInfo.Cellphone, supportInfo.Email);
                        iMessageCourier.Subject = string.Format("Schoombee And Son Order {0}", orderNumber);

                        await email.SendAsync(iMessageCourier, orderDeliveryInstruction.FirstOrDefault().Key);
                    }
                }
                else
                {
                    string smsLine1 = db.SiteConfigurations
                        .Where(c => c.Key == "OrderConfirmationSmsPostOfficeLine1")
                        .Select(c => c.Value)
                        .FirstOrDefault();

                    await helper.SendMessage(string.Format(smsLine1, user.Name, order.OrderNumber,
                        string.Format("{0:d}", dateDispatch))
                        , user.PhoneNumber);

                    string message3 = CreatePostOfficeInstructionsMessage();

                    IdentityMessage iMessageCourier = new IdentityMessage();

                    iMessageCourier.Destination = supportInfo.OrdersEmail;
                    iMessageCourier.Body = string.Format((message3), orderNumber);
                    iMessageCourier.Subject = string.Format("Schoombee And Son Order {0}", orderNumber);

                    await email.SendAsync(iMessageCourier, orderDeliveryInstruction.FirstOrDefault().Key);
                }
            }
        }

        private static string CreatePostOfficeInstructionsMessage()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                string line1 = db.SiteConfigurations
                    .Where(c => c.Key == "OrderPostOfficeLine1")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                string line2 = db.SiteConfigurations
                   .Where(c => c.Key == "OrderPostOfficeLine2")
                   .Select(c => c.Value)
                   .FirstOrDefault();

                string line3 = db.SiteConfigurations
                   .Where(c => c.Key == "OrderPostOfficeLine3")
                   .Select(c => c.Value)
                   .FirstOrDefault();

                return line1 + line2 + line3;
            }
        }

        private static string CreateCourierInstructionsMessage()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                string line1 = db.SiteConfigurations
                    .Where(c => c.Key == "OrderDeliveryInstructionsLine1")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                string line2 = db.SiteConfigurations
                   .Where(c => c.Key == "OrderDeliveryInstructionsLine2")
                   .Select(c => c.Value)
                   .FirstOrDefault();

                string line3 = db.SiteConfigurations
                   .Where(c => c.Key == "OrderDeliveryInstructionsLine3")
                   .Select(c => c.Value)
                   .FirstOrDefault();

                string line4 = db.SiteConfigurations
                   .Where(c => c.Key == "OrderDeliveryInstructionsLine4")
                   .Select(c => c.Value)
                   .FirstOrDefault();

                return line1 + line2 + line3 + line4;
            }
        }

        private static string CreateConfirmationMessageCustomer()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                string line1 = db.SiteConfigurations
                    .Where(c => c.Key == "OrderConfirmationEmailLine1")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                string line2 = db.SiteConfigurations
                    .Where(c => c.Key == "OrderConfirmationEmailLine2")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                string line3 = db.SiteConfigurations
                    .Where(c => c.Key == "OrderConfirmationEmailLine3")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                string line4 = db.SiteConfigurations
                    .Where(c => c.Key == "OrderConfirmationEmailLine4")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                string line5 = db.SiteConfigurations
                    .Where(c => c.Key == "OrderConfirmationEmailLine5")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                return line1 + line2 + line3 + line4 + line5;
            }
        }

        public static DateTime GetDispatchDay(DateTime preferredDeliveryTime)
        {
            DateTime deliveryDate = preferredDeliveryTime;

            while (deliveryDate.DayOfWeek != DayOfWeek.Tuesday)
                deliveryDate = deliveryDate.AddDays(-1);

            return deliveryDate;
        }

        public static DateTime GetArriveDay(DateTime preferredDeliveryTime)
        {
            DateTime today = GetDispatchDay(preferredDeliveryTime);
            int daysUntilFriday = ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;
            DateTime nextFriday = today.AddDays(daysUntilFriday);

            TimeSpan ts = new TimeSpan(preferredDeliveryTime.Hour, preferredDeliveryTime.Minute, 0);
            nextFriday = nextFriday.Date + ts;

            return nextFriday;
        }

        public static DateTime GetSuggestedDeliveryTime()
        {
            DateTime today = DateTime.Today;
            int daysUntilFriday = 0;

            if (GetDaysToMinDate() >= 7)
                daysUntilFriday = (((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7) + 7;
            else
                daysUntilFriday = ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;

            DateTime nextFriday = today.AddDays(daysUntilFriday);
            nextFriday = nextFriday.AddHours(12);

            return nextFriday;
        }

        public static int GetDaysToMinDate()
        {
            DateTime today = DateTime.Today;

            int daysTillMinDate = 0;

            if (today.DayOfWeek == DayOfWeek.Tuesday)
            {
                today = today.AddDays(1);
                daysTillMinDate++;
            }

            if (today.DayOfWeek == DayOfWeek.Wednesday)
            {
                today = today.AddDays(1);
                daysTillMinDate++;
            }

            while (today.DayOfWeek != DayOfWeek.Wednesday)
            {
                today = today.AddDays(1);
                daysTillMinDate++;
            }

            return daysTillMinDate;
        }

        public override string ToString()
        {
            string toString = "";

            if (!string.IsNullOrEmpty(CustomerName) && OrderNumber != 0)
            {
                toString += string.Format("\nOrder Header:\n");
                toString += string.Format(("\nCustomer   :    {0}"), CustomerName);
                toString += string.Format(("\nOrder      :    {0}"), OrderNumber);
                toString += string.Format(("\nTotal      :    {0}\n"), TotalOrderValue);
            }

            return toString;
        }
    }
    public class OrderHeaderMetaData
    {
        [DisplayName("Order Date Placed")]
        public DateTime OrderDatePlaced { get; set; }

        [DisplayName("Order Status")]
        public string OrderStatus { get; set; }

        [DisplayName("Order Number")]
        public int OrderNumber { get; set; }
    }
}