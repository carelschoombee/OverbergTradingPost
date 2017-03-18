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
using System.Text;
using System.Web;

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
                    address = new CustomerAddress();

                int localCourierResult = 0;
                int weightDummy = 1;
                try
                {
                    if (!string.IsNullOrEmpty(address.AddressPostalCode))
                    {
                        localCourierResult = (int)db.CalculateLocalDeliveryFeeAdhoc(weightDummy, int.Parse(address.AddressPostalCode)).FirstOrDefault();
                    }
                }
                catch (Exception e)
                {
                    ExceptionLogging.LogException(e);
                }

                string deliveryType = "";

                if (localCourierResult == -1)
                    deliveryType = "Courier";
                else if (localCourierResult > 0)
                    deliveryType = "LocalCourier";
                else
                    deliveryType = "PostOffice";

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
                        PaymentOption = 1,

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
                        DeliveryType = deliveryType,
                        DateDispatched = null,
                        DateRefunded = null,

                        InvoiceSent = false,

                        CustomerName = user.Name,
                        CustomerEmail = user.Email,
                        CustomerPrimaryContactPhone = user.PhoneNumber,
                        CustomerPreferredCommunicationMethod = user.PreferredCommunicationMethod
                    };

                    //db.OrderHeaders.Add(order);
                    //db.SaveChanges();

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

            if (DeliveryType == "Courier")
                CourierNumber = 1;
            else if (DeliveryType == "LocalCourier")
                CourierNumber = 2;

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                db.Entry(this).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static Dictionary<Stream, string> GetReport(string reportType, int orderNumber)
        {
            Stream stream = new MemoryStream();
            Dictionary<Stream, string> outCollection = new Dictionary<Stream, string>();
            ReportDataSource rds = new ReportDataSource();

            try
            {
                if (reportType == "DeliveryInstructions")
                {
                    GetOrderDeliveryReportTableAdapter ta = new GetOrderDeliveryReportTableAdapter();
                    FreeMarketDataSet ds = new FreeMarketDataSet();

                    ds.GetOrderDeliveryReport.Clear();
                    ds.EnforceConstraints = false;

                    ta.Fill(ds.GetOrderDeliveryReport, orderNumber);

                    rds.Name = "DataSet1";
                    rds.Value = ds.GetOrderDeliveryReport;
                }
                else
                {
                    GetOrderReportTableAdapter ta = new GetOrderReportTableAdapter();
                    FreeMarketDataSet ds = new FreeMarketDataSet();

                    ds.GetOrderReport.Clear();
                    ds.EnforceConstraints = false;

                    ta.Fill(ds.GetOrderReport, orderNumber);

                    rds.Name = "DataSet1";
                    rds.Value = ds.GetOrderReport;
                }

                ReportViewer rv = new ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;

                switch (reportType)
                {
                    case "Invoice":
                        rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report7.rdlc");
                        break;
                    case "Refund":
                        rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report6.rdlc");
                        break;
                    case "PostalInstructions":
                        rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report5.rdlc");
                        break;
                    case "PostalConfirmation":
                        rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report4.rdlc");
                        break;
                    case "DeliveryInstructions":
                        rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report2.rdlc");
                        break;
                    case "OrderConfirmation":
                        rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report1.rdlc");
                        break;
                    default:
                        return new Dictionary<Stream, string>();
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
                ExceptionLogging.LogException(e);
            }

            return outCollection;
        }

        public async static void SendWarningEmail(int orderNumber = 0)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                Support support = db.Supports.FirstOrDefault();
                EmailService email = new EmailService();

                IdentityMessage iMessage = new IdentityMessage();
                iMessage.Destination = support.Email;

                string message1 = "Possible fraudulent activity on order {0}. Checksum failed on payment.";
                string message2 = "Possible fraudulent activity. Reference could not be parsed. Check logs at about this time.";

                if (orderNumber == 0)
                    iMessage.Body = message2;
                else
                    iMessage.Body = string.Format((message1), orderNumber);
                iMessage.Subject = string.Format("Warning!");

                await email.SendAsync(iMessage);
            }
        }

        public async static void SendDispatchMessage(string customerNumber, int orderNumber)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                // Query the database to determine whether dispatch message is activated
                WebsiteFunction function = db.WebsiteFunctions.Find(3);
                if (function != null)
                {
                    if (function.Activated == true)
                    {
                        ApplicationUser user = System.Web.HttpContext
                        .Current
                        .GetOwinContext()
                        .GetUserManager<ApplicationUserManager>()
                        .FindById(customerNumber);

                        OrderHeader order = db.OrderHeaders.Find(orderNumber);

                        if (order == null)
                            return;

                        string deliveryType = order.DeliveryType;
                        if (deliveryType == "PostOffice")
                            deliveryType = "Post Office";

                        string smsLine1 = db.SiteConfigurations
                            .Where(c => c.Key == "OrderDispatchSmsLine1")
                            .Select(c => c.Value)
                            .FirstOrDefault();

                        string smsLine1TrackingNumber = db.SiteConfigurations
                            .Where(c => c.Key == "OrderDispatchSmsLine1TrackingNumber")
                            .Select(c => c.Value)
                            .FirstOrDefault();

                        SMSHelper helper = new SMSHelper();

                        if (string.IsNullOrEmpty(order.TrackingCodes))
                            await helper.SendMessage(string.Format(smsLine1, user.Name, orderNumber, deliveryType), user.PhoneNumber);
                        else
                            await helper.SendMessage(string.Format(smsLine1TrackingNumber, user.Name, orderNumber, deliveryType, order.TrackingCodes), user.PhoneNumber);
                    }
                }
            }
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

                string url = db.SiteConfigurations
                    .Where(c => c.Key == "RatingUrlCustomer")
                    .FirstOrDefault()
                    .Value;

                url = string.Format("{0}?orderNumber={1}", url, orderNumber);

                EmailService email = new EmailService();

                IdentityMessage iMessage = new IdentityMessage();
                iMessage.Destination = user.Email;

                string message1 = CreateRatingMessageCustomer();

                iMessage.Body = string.Format((message1), user.Name, url);
                iMessage.Subject = string.Format("Overberg Trading Post Order");

                await email.SendAsync(iMessage);

                // Query the database to determine whether rating sms is activated
                WebsiteFunction function = db.WebsiteFunctions.Find(4);
                if (function != null)
                {
                    if (function.Activated == true)
                    {
                        SMSHelper helper = new SMSHelper();

                        string smsLine1 = db.SiteConfigurations
                            .Where(c => c.Key == "OrderRateSmsLine1")
                            .Select(c => c.Value)
                            .FirstOrDefault();

                        await helper.SendMessage(string.Format(smsLine1, user.Name, orderNumber, url), user.PhoneNumber);
                    }
                }
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
                Dictionary<Stream, string> refundSummary;

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
                    iMessage.Subject = string.Format("Overberg Trading Post Refund");

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

        public async static void SendInvoice(string customerNumber, int orderNumber)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                OrderHeader order = db.OrderHeaders.Find(orderNumber);

                if (order == null)
                    return;

                ApplicationUser user = System.Web.HttpContext
                            .Current
                            .GetOwinContext()
                            .GetUserManager<ApplicationUserManager>()
                            .FindById(customerNumber);

                if (user == null)
                    return;

                Support supportInfo = db.Supports
                   .FirstOrDefault();

                Dictionary<Stream, string> orderSummary = new Dictionary<Stream, string>();

                orderSummary = GetReport(ReportType.Invoice.ToString(), orderNumber);

                SendInvoiceEmailToCustomer(order, user, supportInfo, orderSummary);

                SendInvoiceSmsToCustomer(user, order);

                SendConfirmationSmsToSupport(user, supportInfo, order);

                order.InvoiceSent = true;
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public async static void SendConfirmationMessages(string customerNumber, int orderNumber)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                OrderHeader order = db.OrderHeaders.Find(orderNumber);

                if (order == null)
                    return;

                ApplicationUser user = System.Web.HttpContext
                            .Current
                            .GetOwinContext()
                            .GetUserManager<ApplicationUserManager>()
                            .FindById(customerNumber);

                if (user == null)
                    return;

                Support supportInfo = db.Supports
                    .FirstOrDefault();

                Dictionary<Stream, string> orderSummary = new Dictionary<Stream, string>();
                Dictionary<Stream, string> orderDeliveryInstruction = new Dictionary<Stream, string>();

                GetConfirmationReports(orderNumber, order.DeliveryType, ref orderSummary, ref orderDeliveryInstruction);

                SendConfirmationEmailToCustomer(order, user, supportInfo, orderSummary);

                SendConfirmationSmsToCustomer(user, order);

                SendConfirmationEmailToCourier(order, supportInfo, orderDeliveryInstruction);
            }
        }

        private async static void SendConfirmationEmailToCourier(OrderHeader order, Support supportInfo, Dictionary<Stream, string> orderDeliveryInstruction)
        {
            if (order.DeliveryType == "Courier" || order.DeliveryType == "LocalCourier")
            {
                Courier courier = new Courier();
                List<GetOrderDeliveryReport_Result> result = new List<GetOrderDeliveryReport_Result>();

                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    courier = db.Couriers.Find(order.CourierNumber);
                    result = db.GetOrderDeliveryReport(order.OrderNumber).ToList();
                }

                string destination = "";
                string body = "";
                string subject = "";
                string cc = "";

                if (courier != null)
                {
                    string message = CreateCourierInstructionsMessage();

                    if (ConfigurationManager.AppSettings["testMode"] == "true")
                        destination = supportInfo.OrdersEmail;
                    else
                        destination = courier.MainContactEmailAddress;

                    string itemsTable = OrderHeader.BuildItemsTableForEmail(result);

                    body = string.Format((message)
                        , result.FirstOrDefault().OrderNumber
                        , result.FirstOrDefault().OrderNumber
                        , result.FirstOrDefault().CustomerName
                        , result.FirstOrDefault().CustomerPhone1
                        , result.FirstOrDefault().CustomerPhone2
                        , result.FirstOrDefault().CustomerEmail
                        , string.Format("{0:f}", result.FirstOrDefault().PreferredDeliveryDate)
                        , result.FirstOrDefault().AddressLine1
                        , result.FirstOrDefault().Suburb
                        , result.FirstOrDefault().City
                        , result.FirstOrDefault().PostalCode
                        , result.FirstOrDefault().StreetAddress
                        , result.FirstOrDefault().TownName
                        , result.FirstOrDefault().StruisbaaiPostalCode
                        , result.FirstOrDefault().Province
                        , itemsTable
                        , supportInfo.MainContactName
                        , supportInfo.Landline
                        , supportInfo.Cellphone
                        , supportInfo.Email);

                    subject = string.Format("Overberg Trading Post Order {0}", order.OrderNumber);

                    if (order.DeliveryType == "Courier")
                        cc = ConfigurationManager.AppSettings["timeFreightManagementEmail"];
                    else
                        cc = string.Empty;

                    EmailService email = new EmailService();

                    await email.SendAsync(subject, destination, cc, body, orderDeliveryInstruction.FirstOrDefault().Key);
                }
            }
            else if (order.DeliveryType == "PostOffice")
            {
                string message = CreatePostOfficeInstructionsMessage();

                IdentityMessage iMessageCourier = new IdentityMessage();

                iMessageCourier.Destination = supportInfo.OrdersEmail;
                iMessageCourier.Body = string.Format((message), order.OrderNumber);
                iMessageCourier.Subject = string.Format("Overberg Trading Post Order {0}", order.OrderNumber);

                EmailService email = new EmailService();

                await email.SendAsync(iMessageCourier, orderDeliveryInstruction.FirstOrDefault().Key);
            }
        }

        private static string BuildItemsTableForEmail(List<GetOrderDeliveryReport_Result> result)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<table>");

            builder.Append("<tr>");
            builder.Append("<th>");
            builder.Append("Item");
            builder.Append("</th>");
            builder.Append("<th>");
            builder.Append("Weight (KG)");
            builder.Append("</th>");
            builder.Append("<th>");
            builder.Append("Size");
            builder.Append("</th>");
            builder.Append("<th>");
            builder.Append("Quantity");
            builder.Append("</th>");
            builder.Append("</tr>");

            foreach (GetOrderDeliveryReport_Result res in result)
            {
                builder.Append("<tr>");
                builder.Append("<td>");
                builder.Append(res.Description);
                builder.Append("</td>");
                builder.Append("<td>");
                builder.Append(Math.Round(res.Weight, 2));
                builder.Append("</td>");
                builder.Append("<td>");
                builder.Append(res.Size);
                builder.Append("</td>");
                builder.Append("<td>");
                builder.Append(res.Quantity);
                builder.Append("</td>");
                builder.Append("</tr>");
            }

            builder.Append("</table>");

            return builder.ToString();
        }

        private async static void SendConfirmationSmsToCustomer(ApplicationUser user, OrderHeader order)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                // Query the database to determine whether confirmation sms to customer is activated
                WebsiteFunction function = db.WebsiteFunctions.Find(2);
                if (function != null)
                {
                    if (function.Activated == true)
                    {
                        if (order.DeliveryType == "Courier" || order.DeliveryType == "LocalCourier")
                        {
                            DateTime dateDispatch = DateTime.Now;
                            DateTime dateArrive = DateTime.Now;

                            dateDispatch = GetDispatchDay((DateTime)order.DeliveryDate);
                            dateArrive = GetArriveDay((DateTime)order.DeliveryDate);

                            string message = "";

                            message = db.SiteConfigurations
                                    .Where(c => c.Key == "OrderConfirmationSmsLine1")
                                    .Select(c => c.Value)
                                    .FirstOrDefault();

                            SMSHelper helper = new SMSHelper();

                            await helper.SendMessage(string.Format(message
                                , user.Name
                                , order.OrderNumber
                                , string.Format("{0:d}", dateDispatch)
                                , string.Format("{0:f}", dateArrive))
                                , user.PhoneNumber);
                        }
                        else if (order.DeliveryType == "PostOffice")
                        {
                            DateTime dateDispatch = DateTime.Now;
                            DateTime dateArrive = DateTime.Now;

                            dateDispatch = GetDispatchDay((DateTime)order.DeliveryDate);
                            dateArrive = GetArriveDay((DateTime)order.DeliveryDate);

                            string message = "";

                            message = db.SiteConfigurations
                                .Where(c => c.Key == "OrderConfirmationSmsPostOfficeLine1")
                                .Select(c => c.Value)
                                .FirstOrDefault();

                            SMSHelper helper = new SMSHelper();

                            await helper.SendMessage(string.Format(message
                                , user.Name
                                , order.OrderNumber
                                , string.Format("{0:d}", dateDispatch))
                                , user.PhoneNumber);
                        }
                        else if (order.DeliveryType == "Virtual")
                        {
                            string message = "";

                            message = db.SiteConfigurations
                                    .Where(c => c.Key == "OrderVirtualConfirmationSmsLine1")
                                    .Select(c => c.Value)
                                    .FirstOrDefault();

                            SMSHelper helper = new SMSHelper();

                            await helper.SendMessage(string.Format(message
                                , user.Name
                                , order.OrderNumber)
                                , user.PhoneNumber);
                        }
                    }
                }
            }
        }

        private async static void SendInvoiceSmsToCustomer(ApplicationUser user, OrderHeader order)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                // Query the database to determine whether invoice payment reminder sms to customer is activated
                WebsiteFunction function = db.WebsiteFunctions.Find(5);
                if (function != null)
                {
                    if (function.Activated == true)
                    {
                        DateTime dateDispatch = DateTime.Now;
                        DateTime dateArrive = DateTime.Now;

                        dateDispatch = GetDispatchDay((DateTime)order.DeliveryDate);
                        dateArrive = GetArriveDay((DateTime)order.DeliveryDate);

                        string message = "";

                        message = db.SiteConfigurations
                                .Where(c => c.Key == "InvoiceSmsLine1")
                                .Select(c => c.Value)
                                .FirstOrDefault();

                        SMSHelper helper = new SMSHelper();

                        await helper.SendMessage(string.Format(message
                            , user.Name
                            , order.OrderNumber)
                            , user.PhoneNumber);
                    }
                }
            }
        }

        private async static void SendConfirmationSmsToSupport(ApplicationUser user, Support supportInfo, OrderHeader order)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                // Query database if sms to support is activated
                WebsiteFunction function = db.WebsiteFunctions.Find(1);
                if (function != null)
                {
                    if (function.Activated == true)
                    {
                        string message = "";
                        string date = "";

                        if (order.DeliveryDate == null)
                            date = "Not Applicable";
                        else
                            date = string.Format("{0:f}", order.DeliveryDate);

                        message = db.SiteConfigurations
                            .Where(c => c.Key == "OrderConfirmationSMSSupport")
                            .FirstOrDefault()
                            .Value;

                        SMSHelper helper = new SMSHelper();

                        await helper.SendMessage(
                                string.Format(message
                                , user.Name, order.OrderNumber
                                , date
                                , string.Format("{0:C}", order.TotalOrderValue)
                                , order.DeliveryType)
                            , string.Format("{0},{1}"
                                , supportInfo.Cellphone
                                , supportInfo.Landline));
                    }
                }
            }
        }

        private async static void SendConfirmationEmailToCustomer(OrderHeader order, ApplicationUser user, Support supportInfo, Dictionary<Stream, string> orderSummary)
        {
            IdentityMessage iMessage = new IdentityMessage();
            iMessage.Destination = user.Email;

            string message1 = CreateConfirmationMessageCustomer();

            iMessage.Body = string.Format((message1), user.Name, supportInfo.MainContactName, supportInfo.Landline, supportInfo.Cellphone, supportInfo.Email);
            iMessage.Subject = string.Format("Overberg Trading Post Order");

            EmailService email = new EmailService();

            await email.SendAsync(iMessage, orderSummary.FirstOrDefault().Key);
        }

        private async static void SendInvoiceEmailToCustomer(OrderHeader order, ApplicationUser user, Support supportInfo, Dictionary<Stream, string> orderSummary)
        {
            IdentityMessage iMessage = new IdentityMessage();
            iMessage.Destination = user.Email;

            string message1 = CreateInvoiceMessageCustomer();

            iMessage.Body = string.Format((message1), user.Name, supportInfo.MainContactName, supportInfo.Landline, supportInfo.Cellphone, supportInfo.Email);
            iMessage.Subject = string.Format("Overberg Trading Post Order");

            EmailService email = new EmailService();

            await email.SendAsync(iMessage, orderSummary.FirstOrDefault().Key);
        }

        private static void GetConfirmationReports(int orderNumber, string deliveryType, ref Dictionary<Stream,
            string> orderSummary, ref Dictionary<Stream, string> orderDeliveryInstruction)
        {
            if (deliveryType == "Courier" || deliveryType == "LocalCourier" || deliveryType == "Virtual")
            {
                orderSummary = GetReport(ReportType.OrderConfirmation.ToString(), orderNumber);
                orderDeliveryInstruction = GetReport(ReportType.DeliveryInstructions.ToString(), orderNumber);
            }
            else if (deliveryType == "PostOffice")
            {
                orderSummary = GetReport(ReportType.PostalConfirmation.ToString(), orderNumber);
                orderDeliveryInstruction = GetReport(ReportType.PostalInstructions.ToString(), orderNumber);
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

        private static string CreateInvoiceMessageCustomer()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                string line1 = db.SiteConfigurations
                    .Where(c => c.Key == "InvoiceLine1")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                string line2 = db.SiteConfigurations
                    .Where(c => c.Key == "InvoiceLine2")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                string line3 = db.SiteConfigurations
                    .Where(c => c.Key == "InvoiceLine3")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                string line4 = db.SiteConfigurations
                    .Where(c => c.Key == "InvoiceLine4")
                    .Select(c => c.Value)
                    .FirstOrDefault();

                return line1 + line2 + line3 + line4;
            }
        }

        public static DateTime GetDispatchDay(DateTime preferredDeliveryTime)
        {
            DateTime deliveryDate = preferredDeliveryTime;

            if (deliveryDate.DayOfWeek == DayOfWeek.Tuesday)
            {
                deliveryDate = deliveryDate.AddDays(7);
            }
            else
            {
                while (deliveryDate.DayOfWeek != DayOfWeek.Tuesday && deliveryDate > DateTime.Now)
                    deliveryDate = deliveryDate.AddDays(-1);
            }

            return deliveryDate;
        }

        public static DateTime GetArriveDay(DateTime preferredDeliveryTime)
        {
            DateTime today = GetDispatchDay(preferredDeliveryTime);
            int daysUntilFriday = 0;

            if (today.DayOfWeek == DayOfWeek.Friday)
            {
                daysUntilFriday = 7;
            }
            else
            {
                daysUntilFriday = ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;
            }

            DateTime nextFriday = today.AddDays(daysUntilFriday);

            TimeSpan ts = new TimeSpan(preferredDeliveryTime.Hour, preferredDeliveryTime.Minute, 0);
            nextFriday = nextFriday.Date + ts;

            return nextFriday;
        }

        public static DateTime GetSpecialDispatchDay(DateTime preferredDeliveryTime)
        {
            DateTime dispatchDate = preferredDeliveryTime;

            if (dispatchDate.DayOfWeek == DayOfWeek.Monday)
            {
                return dispatchDate;
            }

            if (dispatchDate.DayOfWeek == DayOfWeek.Tuesday)
            {
                dispatchDate = dispatchDate.AddDays(-1);
                return dispatchDate;
            }

            dispatchDate = dispatchDate.AddDays(-2);

            while (dispatchDate < DateTime.Now)
            {
                dispatchDate = dispatchDate.AddDays(1);
            }

            return dispatchDate;
        }

        public static DateTime GetSpecialArriveDay(DateTime preferredDeliveryTime)
        {
            return preferredDeliveryTime;
        }

        public static DateTime GetSuggestedDeliveryTime()
        {
            DateTime today = DateTime.Today;
            int daysUntilFriday = 0;

            if (today.DayOfWeek == DayOfWeek.Monday)
                daysUntilFriday = 4;
            else if (today.DayOfWeek == DayOfWeek.Tuesday)
                daysUntilFriday = 10;
            else if (today.DayOfWeek == DayOfWeek.Wednesday)
                daysUntilFriday = 9;
            else if (today.DayOfWeek == DayOfWeek.Thursday)
                daysUntilFriday = 8;
            else if (today.DayOfWeek == DayOfWeek.Friday)
                daysUntilFriday = 7;
            else if (today.DayOfWeek == DayOfWeek.Saturday)
                daysUntilFriday = 6;
            else if (today.DayOfWeek == DayOfWeek.Sunday)
                daysUntilFriday = 5;

            DateTime nextFriday = today.AddDays(daysUntilFriday);
            nextFriday = nextFriday.AddHours(12);

            return nextFriday;
        }

        public static DateTime GetSpecialSuggestedDeliveryTime()
        {
            DateTime today = DateTime.Today;
            DateTime suggestedDate = today.AddDays(2);

            while (suggestedDate.DayOfWeek == DayOfWeek.Saturday || suggestedDate.DayOfWeek == DayOfWeek.Sunday)
            {
                suggestedDate = suggestedDate.AddDays(1);
            }

            suggestedDate = suggestedDate.AddHours(12);

            return suggestedDate;
        }

        //#region testing

        //public static void TestDates()
        //{
        //    DateTime date1 = DateTime.Now;
        //    DateTime date2 = date1;

        //    int i = 0;

        //    while (i < 14)
        //    {
        //        //while (date2.DayOfWeek != DayOfWeek.Wednesday && date2.DayOfWeek != DayOfWeek.Thursday && date2.DayOfWeek != DayOfWeek.Friday)
        //        //{
        //        //    date2 = date2.AddDays(1);
        //        //}

        //        //Debug.WriteLine("-------------------------------------");
        //        //Debug.WriteLine("date2      : {0}", date2);
        //        //Debug.WriteLine("Dispatch   : {0}", GetDispatchDay(date2));
        //        //Debug.WriteLine("Arrive     : {0}", GetArriveDay(date2));
        //        //Debug.WriteLine("-------------------------------------");

        //        Debug.WriteLine("-------------------------------------");
        //        Debug.WriteLine("date2      : {0}", date2);
        //        Debug.WriteLine("Dispatch   : {0}", GetSpecialDispatchDay(date2));
        //        Debug.WriteLine("Arrive     : {0}", GetSpecialArriveDay(date2));
        //        Debug.WriteLine("-------------------------------------");

        //        date2 = date2.AddDays(1);

        //        i++;
        //    }

        //    i = 0;

        //    Debug.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");

        //    date2 = date1;

        //    while (i < 14)
        //    {
        //        date2 = date2.AddDays(1);

        //        Debug.WriteLine("-------------------------------------");
        //        Debug.WriteLine("date2                      : {0}", date2);
        //        Debug.WriteLine("SuggestedDelivery          : {0}", GetSuggestedDeliveryTimeTest(date2));
        //        Debug.WriteLine("SuggestedSpecialDelivery   : {0}", GetSpecialSuggestedDeliveryTimeTest(date2));
        //        Debug.WriteLine("DaysToMinDate              : {0}", GetDaysToMinDateTest(date2));
        //        Debug.WriteLine("-------------------------------------");

        //        i++;
        //    }
        //}

        //public static DateTime GetSpecialSuggestedDeliveryTimeTest(DateTime todayTest)
        //{
        //    DateTime today = todayTest;
        //    DateTime suggestedDate = today.AddDays(2);

        //    while (suggestedDate.DayOfWeek == DayOfWeek.Saturday || suggestedDate.DayOfWeek == DayOfWeek.Sunday)
        //    {
        //        suggestedDate = suggestedDate.AddDays(1);
        //    }

        //    suggestedDate = suggestedDate.AddHours(-12);

        //    return suggestedDate;
        //}

        //public static DateTime GetSuggestedDeliveryTimeTest(DateTime todayDate)
        //{
        //    DateTime today = todayDate;
        //    int daysUntilFriday = 0;

        //    if (today.DayOfWeek == DayOfWeek.Friday)
        //        daysUntilFriday = 7;
        //    else
        //    {
        //        if (GetDaysToMinDate() >= 6)
        //            daysUntilFriday = (((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7) + 7;
        //        else
        //            daysUntilFriday = ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;
        //    }

        //    DateTime nextFriday = today.AddDays(daysUntilFriday);
        //    nextFriday = nextFriday.AddHours(-12);

        //    return nextFriday;
        //}

        //public static int GetDaysToMinDateTest(DateTime todayDay)
        //{
        //    DateTime today = todayDay;

        //    int daysTillMinDate = 0;

        //    if (today.DayOfWeek == DayOfWeek.Monday)
        //    {
        //        daysTillMinDate = 1;
        //        return daysTillMinDate;
        //    }

        //    if (today.DayOfWeek == DayOfWeek.Tuesday)
        //    {
        //        daysTillMinDate = 7;
        //        return daysTillMinDate;
        //    }

        //    if (today.DayOfWeek == DayOfWeek.Wednesday)
        //    {
        //        daysTillMinDate = 6;
        //        return daysTillMinDate;
        //    }

        //    if (today.DayOfWeek == DayOfWeek.Thursday)
        //    {
        //        daysTillMinDate = 5;
        //        return daysTillMinDate;
        //    }

        //    if (today.DayOfWeek == DayOfWeek.Friday)
        //    {
        //        daysTillMinDate = 4;
        //        return daysTillMinDate;
        //    }

        //    if (today.DayOfWeek == DayOfWeek.Saturday)
        //    {
        //        daysTillMinDate = 3;
        //        return daysTillMinDate;
        //    }

        //    if (today.DayOfWeek == DayOfWeek.Sunday)
        //    {
        //        daysTillMinDate = 2;
        //        return daysTillMinDate;
        //    }

        //    return 0;
        //}
        //#endregion

        public static int GetDaysToMinDate()
        {
            DateTime today = DateTime.Today;

            int daysTillMinDate = 0;

            if (today.DayOfWeek == DayOfWeek.Monday)
            {
                daysTillMinDate = 1;
                return daysTillMinDate;
            }

            if (today.DayOfWeek == DayOfWeek.Tuesday)
            {
                daysTillMinDate = 7;
                return daysTillMinDate;
            }

            if (today.DayOfWeek == DayOfWeek.Wednesday)
            {
                daysTillMinDate = 6;
                return daysTillMinDate;
            }

            if (today.DayOfWeek == DayOfWeek.Thursday)
            {
                daysTillMinDate = 5;
                return daysTillMinDate;
            }

            if (today.DayOfWeek == DayOfWeek.Friday)
            {
                daysTillMinDate = 4;
                return daysTillMinDate;
            }

            if (today.DayOfWeek == DayOfWeek.Saturday)
            {
                daysTillMinDate = 3;
                return daysTillMinDate;
            }

            if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                daysTillMinDate = 2;
                return daysTillMinDate;
            }

            return 0;
        }

        public static List<GetDeliveryLabels_Result> GetDeliveryLabels()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                return db.GetDeliveryLabels().ToList();
            }
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

        public static List<OrderHeader> RefreshInvoiceCollection()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                return db.OrderHeaders
                    .Where(c => c.OrderStatus == "Invoiced")
                    .OrderByDescending(c => c.OrderNumber)
                    .ToList();
            }
        }

        public static List<OrderHeader> RefreshConfirmedCollection()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                return db.OrderHeaders
                    .Where(c => c.OrderStatus == "Confirmed")
                    .OrderByDescending(c => c.OrderNumber)
                    .ToList();
            }
        }

        public static List<OrderHeader> RefreshInTransitCollection()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                return db.OrderHeaders
                    .Where(c => c.OrderStatus == "InTransit")
                    .OrderByDescending(c => c.OrderNumber)
                    .ToList();
            }
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

        [DisplayName("Tracking Codes")]
        [StringLength(700)]
        public int TrackingCodes { get; set; }
    }
}