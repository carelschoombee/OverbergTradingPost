using FreeMarket.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace FreeMarket.Models
{
    public class PaymentGatewayIntegration
    {
        static HttpClient client = new HttpClient();

        public string BaseUri { get; set; }
        public string Url { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Message1 { get; set; }
        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public string CustomerEmail { get; set; }
        public PaymentGatewayParameter Parameters { get; set; }
        public string Pay_Request_Id { get; set; }
        public string Checksum { get; set; }

        public PaymentGatewayIntegration(string reference, decimal amount, string customerEmail)
        {
            BaseUri = "https://secure.paygate.co.za/";
            Url = "/payweb3/initiate.trans";
            Reference = reference;
            Amount = amount;
            CustomerEmail = customerEmail;

            Parameters = new PaymentGatewayParameter();
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                Parameters = db.PaymentGatewayParameters
                    .Where(c => c.PaymentGatewayName == "PayGate")
                    .FirstOrDefault();
            }

        }

        public static PaymentGatewayParameter GetParameters()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                return db.PaymentGatewayParameters
                    .Where(c => c.PaymentGatewayName == "PayGate")
                    .FirstOrDefault();
            }
        }

        public void Execute()
        {
            CreateTestMessage1();

            bool checksumPassed = false;
            string response1 = SendMessage(Message1);

            if (!string.IsNullOrEmpty(response1))
            {
                var nvc = HttpUtility.ParseQueryString(response1);
                string PAYGATE_ID = nvc["PAYGATE_ID"];
                string PAY_REQUEST_ID = nvc["PAY_REQUEST_ID"];
                string REFERENCE = nvc["REFERENCE"];
                string CHECKSUM = nvc["CHECKSUM"];

                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    string check = PAYGATE_ID + PAY_REQUEST_ID + REFERENCE + Parameters.Key;
                    string checkTransaction = Extensions.CreateMD5(check);
                    if (CHECKSUM == checkTransaction)
                        checksumPassed = true;

                    Pay_Request_Id = PAY_REQUEST_ID;
                    Checksum = CHECKSUM;

                    PaymentGatewayMessage message = new PaymentGatewayMessage
                    {
                        PayGate_ID = Convert.ToDecimal(PAYGATE_ID),
                        Reference = REFERENCE,
                        Pay_Request_ID = PAY_REQUEST_ID,
                        Checksum_Passed = checksumPassed
                    };

                    db.PaymentGatewayMessages.Add(message);
                    db.SaveChanges();
                }
            }
        }

        public void CreateMessage1()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                string PAYGATE_ID = Parameters.PaymentGatewayID.ToString();
                string REFERENCE = Reference;
                string AMOUNT = Amount.ToString();
                string CURRENCY = "ZAR";
                string RETURN_URL = Parameters.Return_Url;
                string TRANSACTION_DATE = DateTime.Now.ToString();
                string LOCALE = "en";
                string COUNTRY = "ZAF";
                string EMAIL = CustomerEmail;
                string KEY = Parameters.Key;

                string hash = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}", PAYGATE_ID, REFERENCE, AMOUNT, CURRENCY, RETURN_URL, TRANSACTION_DATE, LOCALE, COUNTRY, EMAIL, KEY);
                string checkSum = Extensions.CreateMD5(hash);

                PaymentGatewayMessage message = new PaymentGatewayMessage
                {
                    PayGate_ID = Parameters.PaymentGatewayID,
                    Reference = REFERENCE,
                    Amount = Amount,
                    Currency = CURRENCY,
                    ReturnUrl = RETURN_URL,
                    Transaction_Date = TRANSACTION_DATE,
                    Locale = LOCALE,
                    Country = COUNTRY,
                    Email = EMAIL
                };

                db.PaymentGatewayMessages.Add(message);
                db.SaveChanges();

                Message1 = new[] {
                        new KeyValuePair<string, string>("PAYGATE_ID", PAYGATE_ID),
                        new KeyValuePair<string, string>("REFERENCE", REFERENCE),
                        new KeyValuePair<string, string>("AMOUNT", AMOUNT),
                        new KeyValuePair<string, string>("CURRENCY", CURRENCY),
                        new KeyValuePair<string, string>("RETURN_URL", RETURN_URL),
                        new KeyValuePair<string, string>("TRANSACTION_DATE", TRANSACTION_DATE),
                        new KeyValuePair<string, string>("LOCALE", LOCALE),
                        new KeyValuePair<string, string>("COUNTRY", COUNTRY),
                        new KeyValuePair<string, string>("EMAIL", EMAIL),
                        new KeyValuePair<string, string>("CHECKSUM", checkSum)
                    };
            }
        }

        public string SendMessage(IEnumerable<KeyValuePair<string, string>> message)
        {
            string resultContent = "";

            try
            {
                client.BaseAddress = new Uri(BaseUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new FormUrlEncodedContent(
                    message
                );

                var result = client.PostAsync(Url, content).Result;
                resultContent = result.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                ExceptionLogging.LogException(e);
                client = new HttpClient();
            }

            return resultContent;
        }

        public void CreateTestMessage1()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                string PAYGATE_ID = Parameters.PaymentGatewayID.ToString();
                string REFERENCE = "PayGate Test";
                string AMOUNT = "3299";
                string CURRENCY = "ZAR";
                string RETURN_URL = Parameters.Return_Url;
                string TRANSACTION_DATE = "2016-03-10 10:49:16";
                string LOCALE = "en";
                string COUNTRY = "ZAF";
                string EMAIL = "customer@paygate.co.za";
                string KEY = "secret";

                string hash = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}", PAYGATE_ID, REFERENCE, AMOUNT, CURRENCY, RETURN_URL, TRANSACTION_DATE, LOCALE, COUNTRY, EMAIL, KEY);
                string checkSum = Extensions.CreateMD5(hash);

                PaymentGatewayMessage message = new PaymentGatewayMessage
                {
                    PayGate_ID = Parameters.PaymentGatewayID,
                    Reference = REFERENCE,
                    Amount = Amount,
                    Currency = CURRENCY,
                    ReturnUrl = RETURN_URL,
                    Transaction_Date = TRANSACTION_DATE,
                    Locale = LOCALE,
                    Country = COUNTRY,
                    Email = EMAIL
                };

                db.PaymentGatewayMessages.Add(message);
                db.SaveChanges();

                Message1 = new[] {
                        new KeyValuePair<string, string>("PAYGATE_ID", PAYGATE_ID),
                        new KeyValuePair<string, string>("REFERENCE", REFERENCE),
                        new KeyValuePair<string, string>("AMOUNT", AMOUNT),
                        new KeyValuePair<string, string>("CURRENCY", CURRENCY),
                        new KeyValuePair<string, string>("RETURN_URL", RETURN_URL),
                        new KeyValuePair<string, string>("TRANSACTION_DATE", TRANSACTION_DATE),
                        new KeyValuePair<string, string>("LOCALE", LOCALE),
                        new KeyValuePair<string, string>("COUNTRY", COUNTRY),
                        new KeyValuePair<string, string>("EMAIL", EMAIL),
                        new KeyValuePair<string, string>("CHECKSUM", checkSum)
                    };
            }
        }
    }
}