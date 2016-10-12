using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeMarket.Models
{
    public class SalesInfo
    {
        public decimal TotalSalesGateway { get; set; }
        public decimal TotalSalesOrders { get; set; }
        public decimal TotalShippingOrders { get; set; }
        public Dictionary<string, decimal> SalesDetails { get; set; }
        public Dictionary<string, int> TransactionDetails { get; set; }
        public Dictionary<string, int> PaymentDetails { get; set; }

        public int TotalSuccessfulOrders { get; set; }
        public int TotalCancelledOrders { get; set; }
        public int TotalLockedOrders { get; set; }
        public int TotalUnconfirmedOrders { get; set; }

        public Dictionary<string, decimal> CalculateSalesDetails(int year, List<OrderHeader> orders)
        {
            Dictionary<string, decimal> sales;

            if (year == 0)
            {
                int minDate = orders.Min(c => c.OrderDatePlaced).Value.Year - 1;
                int maxDate = orders.Max(c => c.OrderDatePlaced).Value.Year;

                sales = new Dictionary<string, decimal>();

                int i = minDate;

                while (i <= maxDate)
                {
                    sales.Add(i.ToString(), 0);
                    i++;
                }

                foreach (OrderHeader o in orders)
                {
                    if (sales.ContainsKey(((DateTime)o.OrderDatePlaced).Year.ToString()))
                    {
                        sales[((DateTime)o.OrderDatePlaced).Year.ToString()] += o.TotalOrderValue;
                    }
                }
            }
            else
            {
                sales = new Dictionary<string, decimal>()
                    {
                        { "January", 0},
                        { "February", 0},
                        { "March", 0},
                        { "April", 0},
                        { "May", 0 },
                        { "June",0 },
                        { "July",0 },
                        { "August",0 },
                        { "September",0 },
                        { "October", 0 },
                        { "November", 0 },
                        { "December", 0 }
                    };

                foreach (OrderHeader o in orders)
                {
                    if (sales.ContainsKey(((DateTime)o.OrderDatePlaced).ToString("MMMM")) && (o.OrderDatePlaced.Value.Year == year))
                    {
                        sales[((DateTime)o.OrderDatePlaced).ToString("MMMM")] += o.TotalOrderValue;
                    }
                }
            }

            return sales;
        }

        public Dictionary<string, int> GetTransactionDetails(List<PaymentGatewayMessage> messages)
        {
            List<PaymentGatewayTransactionStatu> statuses = new List<PaymentGatewayTransactionStatu>();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                statuses = db.PaymentGatewayTransactionStatus.ToList();
            }

            Dictionary<string, int> tempTranscationCodes = new Dictionary<string, int>();
            Dictionary<string, int> transactionCodes = new Dictionary<string, int>();

            foreach (PaymentGatewayTransactionStatu status in statuses)
            {
                tempTranscationCodes.Add(status.TransactionCode.ToString(), 0);
            }

            foreach (PaymentGatewayMessage o in messages)
            {
                if (o.TransactionStatus != null)
                {
                    if (tempTranscationCodes.ContainsKey(o.TransactionStatus.ToString()))
                    {
                        tempTranscationCodes[o.TransactionStatus.ToString()] += 1;
                    }
                }
            }

            foreach (PaymentGatewayTransactionStatu status in statuses)
            {
                transactionCodes.Add(status.Status, 0);
                transactionCodes[status.Status] = tempTranscationCodes[status.TransactionCode.ToString()];
            }

            return transactionCodes;
        }

        public Dictionary<string, int> GetPaymentDetails(List<PaymentGatewayMessage> messages)
        {
            List<PaymentGatewayPaymentMethod> methods = new List<PaymentGatewayPaymentMethod>();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                methods = db.PaymentGatewayPaymentMethods.ToList();
            }

            Dictionary<string, int> tempPaymentMethods = new Dictionary<string, int>();
            Dictionary<string, int> paymentMethods = new Dictionary<string, int>();

            foreach (PaymentGatewayPaymentMethod method in methods)
            {
                tempPaymentMethods.Add(method.PayMethod.ToString(), 0);
            }

            foreach (PaymentGatewayMessage o in messages)
            {
                if (o.Pay_Method != null)
                {
                    if (tempPaymentMethods.ContainsKey(o.Pay_Method.ToString()))
                    {
                        tempPaymentMethods[o.Pay_Method.ToString()] += 1;
                    }
                }
            }

            foreach (PaymentGatewayPaymentMethod method in methods)
            {
                paymentMethods.Add(method.Description, 0);
                paymentMethods[method.Description] = tempPaymentMethods[method.PayMethod.ToString()];
            }

            return paymentMethods;
        }

        public SalesInfo(int year)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                List<OrderHeader> orders = new List<OrderHeader>();
                List<PaymentGatewayMessage> messages = new List<PaymentGatewayMessage>();

                if (year == 0)
                {
                    messages = db.PaymentGatewayMessages.Where(c => c.TransactionStatus != null).ToList();

                    TotalSalesGateway = messages
                        .Where(c => c.TransactionStatus == 1)
                        .Sum(c => c.Amount) ?? 0;

                    TotalSalesGateway = TotalSalesGateway / 100;

                    orders = db.OrderHeaders
                        .Where(c => (c.OrderStatus == "Confirmed" || c.OrderStatus == "Completed"))
                        .ToList();

                    TotalSuccessfulOrders = orders.Count();

                    List<OrderHeader> tempCancelled = db.OrderHeaders.Where(c => c.OrderStatus == "Cancelled").ToList();
                    TotalCancelledOrders = tempCancelled.Count;

                    List<OrderHeader> tempLocked = db.OrderHeaders.Where(c => c.OrderStatus == "Locked").ToList();
                    TotalCancelledOrders = tempLocked.Count;

                    List<OrderHeader> tempUnconfirmed = db.OrderHeaders.Where(c => c.OrderStatus == "Unconfirmed").ToList();
                    TotalCancelledOrders = tempUnconfirmed.Count;
                }
                else
                {
                    try
                    {
                        orders = db.OrderHeaders
                           .Where(c => (c.OrderStatus == "Confirmed" || c.OrderStatus == "Completed")
                           && c.OrderDatePlaced.Value.Year == year)
                           .ToList();

                        messages = db.PaymentGatewayMessages
                            .Where(c => c.TransactionStatus != null && c.Transaction_Date != null)
                            .ToList();

                        TotalSalesGateway = messages
                            .Where(c => c.TransactionStatus == 1 && DateTime.Parse(c.Transaction_Date).Year == year)
                            .Sum(c => c.Amount) ?? 0;

                        TotalSuccessfulOrders = orders.Count;

                        List<OrderHeader> tempCancelled = db.OrderHeaders.Where(c => c.OrderStatus == "Cancelled" && c.OrderDatePlaced.Value.Year == year).ToList();
                        TotalCancelledOrders = tempCancelled.Count;

                        List<OrderHeader> tempLocked = db.OrderHeaders.Where(c => c.OrderStatus == "Locked" && c.OrderDatePlaced.Value.Year == year).ToList();
                        TotalCancelledOrders = tempLocked.Count;

                        List<OrderHeader> tempUnconfirmed = db.OrderHeaders.Where(c => c.OrderStatus == "Unconfirmed" && c.OrderDatePlaced.Value.Year == year).ToList();
                        TotalCancelledOrders = tempUnconfirmed.Count;

                        TotalSalesGateway = TotalSalesGateway / 100;
                    }
                    catch (Exception e)
                    {
                        ExceptionLogging.LogException(e);
                    }
                }

                TotalSalesOrders = orders.Sum(c => c.TotalOrderValue);

                TotalShippingOrders = (decimal)orders.Sum(c => c.ShippingTotal);

                if (orders.Count == 0)
                {
                    SalesDetails = new Dictionary<string, decimal>();
                    return;
                }

                SalesDetails = CalculateSalesDetails(year, orders);
                TransactionDetails = GetTransactionDetails(messages);
                PaymentDetails = GetPaymentDetails(messages);
            }
        }
    }
}