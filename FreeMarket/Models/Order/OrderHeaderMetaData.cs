using FreeMarket.FreeMarketDataSetTableAdapters;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

        public static Dictionary<MemoryStream, string> GetRefundReport(int orderNumber)
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
                rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report6.rdlc");

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

        public static Dictionary<MemoryStream, string> GetOrderReport(int orderNumber)
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
                rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report1.rdlc");

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

        public static Dictionary<MemoryStream, string> GetDeliveryInstructions(int orderNumber)
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
                rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report2.rdlc");

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

        public static Dictionary<MemoryStream, string> GetStruisbaaiOrderReport(int orderNumber)
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
                rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report3.rdlc");

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

        public static Dictionary<MemoryStream, string> GetPostalReport(int orderNumber)
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
                rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report4.rdlc");

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

        public static Dictionary<MemoryStream, string> GetPostalInstructions(int orderNumber)
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
                rv.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/Report5.rdlc");

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

                EmailService email = new EmailService();

                IdentityMessage iMessage = new IdentityMessage();
                iMessage.Destination = user.Email;

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

                var requestContext = HttpContext.Current.Request.RequestContext;
                string url = "https://www.schoombeeandson.co.za" + new UrlHelper(requestContext).Action("RateOrder", "Manage", new { orderNumber = orderNumber });

                iMessage.Body = string.Format((line1 + line2 + line3 + line4 + line5 + line6), user.Name, url);
                iMessage.Subject = string.Format("Schoombee and Son Order");

                await email.SendAsync(iMessage);
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

                    refundSummary = GetRefundReport(orderNumber);

                    ApplicationUser user = System.Web.HttpContext
                                .Current
                                .GetOwinContext()
                                .GetUserManager<ApplicationUserManager>()
                                .FindById(customerNumber);

                    EmailService email = new EmailService();

                    IdentityMessage iMessage = new IdentityMessage();
                    iMessage.Destination = user.Email;

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

                    Support supportInfo = db.Supports
                        .FirstOrDefault();

                    iMessage.Body = string.Format((line1 + line2 + line3 + line4), user.Name, orderNumber, supportInfo.Cellphone, supportInfo.Landline, supportInfo.Email);
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
                }
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
                    orderSummary = GetStruisbaaiOrderReport(orderNumber);
                    orderDeliveryInstruction = GetStruisbaaiOrderReport(orderNumber);
                    specialDelivery = true;
                }
                else
                {
                    if (order.DeliveryType == "Courier")
                    {
                        orderSummary = GetOrderReport(orderNumber);
                        orderDeliveryInstruction = GetDeliveryInstructions(orderNumber);
                    }
                    else
                    {
                        orderSummary = GetPostalReport(orderNumber);
                        orderDeliveryInstruction = GetPostalInstructions(orderNumber);
                    }
                }


                EmailService email = new EmailService();

                IdentityMessage iMessage = new IdentityMessage();
                iMessage.Destination = user.Email;

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

                Support supportInfo = db.Supports
                    .FirstOrDefault();

                iMessage.Body = string.Format((line1 + line2 + line3 + line4 + line5), user.Name, supportInfo.MainContactName, supportInfo.Landline, supportInfo.Cellphone, supportInfo.Email);
                iMessage.Subject = string.Format("Schoombee and Son Order");

                await email.SendAsync(iMessage, orderSummary.FirstOrDefault().Key);

                if (order.DeliveryType == "Courier")
                {
                    Courier courier = db.Couriers.Find(1);

                    if (courier != null)
                    {
                        line1 = db.SiteConfigurations
                       .Where(c => c.Key == "OrderDeliveryInstructionsLine1")
                       .Select(c => c.Value)
                       .FirstOrDefault();

                        line2 = db.SiteConfigurations
                           .Where(c => c.Key == "OrderDeliveryInstructionsLine2")
                           .Select(c => c.Value)
                           .FirstOrDefault();

                        line3 = db.SiteConfigurations
                           .Where(c => c.Key == "OrderDeliveryInstructionsLine3")
                           .Select(c => c.Value)
                           .FirstOrDefault();

                        line4 = db.SiteConfigurations
                           .Where(c => c.Key == "OrderDeliveryInstructionsLine4")
                           .Select(c => c.Value)
                           .FirstOrDefault();

                        IdentityMessage iMessageCourier = new IdentityMessage();

                        if (specialDelivery)
                        {
                            iMessageCourier.Destination = supportInfo.OrdersEmail;
                        }
                        else
                        {
                            iMessageCourier.Destination = courier.MainContactEmailAddress;
                        }

                        iMessageCourier.Body = string.Format((line1 + line2 + line3 + line4), orderNumber, supportInfo.MainContactName, supportInfo.Landline, supportInfo.Cellphone, supportInfo.Email);
                        iMessageCourier.Subject = string.Format("Schoombee And Son Order {0}", orderNumber);

                        await email.SendAsync(iMessageCourier, orderDeliveryInstruction.FirstOrDefault().Key);
                    }
                }
                else
                {
                    line1 = db.SiteConfigurations
                       .Where(c => c.Key == "OrderPostOfficeLine1")
                       .Select(c => c.Value)
                       .FirstOrDefault();

                    line2 = db.SiteConfigurations
                       .Where(c => c.Key == "OrderPostOfficeLine2")
                       .Select(c => c.Value)
                       .FirstOrDefault();

                    line3 = db.SiteConfigurations
                       .Where(c => c.Key == "OrderPostOfficeLine3")
                       .Select(c => c.Value)
                       .FirstOrDefault();

                    IdentityMessage iMessageCourier = new IdentityMessage();

                    iMessageCourier.Destination = supportInfo.OrdersEmail;
                    iMessageCourier.Body = string.Format((line1 + line2 + line3), orderNumber);
                    iMessageCourier.Subject = string.Format("Schoombee And Son Order {0}", orderNumber);

                    await email.SendAsync(iMessageCourier, orderDeliveryInstruction.FirstOrDefault().Key);
                }
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