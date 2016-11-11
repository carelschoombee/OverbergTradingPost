using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FreeMarket.Models
{
    public class SMSHelper
    {
        static HttpClient client = new HttpClient();
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }

        public SMSHelper()
        {
            UserName = ConfigurationManager.AppSettings["smsUserName"];
            Password = ConfigurationManager.AppSettings["smsPassword"];
            Url = ConfigurationManager.AppSettings["smsUrl"];
        }

        public async Task<string> SendMessage(string message, string recipientNumber)
        {
            IEnumerable<KeyValuePair<string, string>> data = new[]
            {
                new KeyValuePair<string, string>("Type", "sendparam"),
                new KeyValuePair<string, string>("Username", UserName),
                new KeyValuePair<string, string>("Password", Password),
                new KeyValuePair<string, string>("numto", recipientNumber),
                new KeyValuePair<string, string>("data1", message),
            };

            string result = await QuerySmsServer(data);
            return result;
        }

        public async Task<string> CheckCredits()
        {
            IEnumerable<KeyValuePair<string, string>> data = new[]
            {
                new KeyValuePair<string, string>("Type", "credits"),
                new KeyValuePair<string, string>("Username", UserName),
                new KeyValuePair<string, string>("Password", Password),
            };

            string result = await QuerySmsServer(data);

            var xdoc = XDocument.Parse(result);
            var credits = xdoc.Descendants()
                .Where(a => a.Name.LocalName == "credits")
                .FirstOrDefault().Value;

            return credits;
        }

        // query API server and return response in object format
        private async Task<string> QuerySmsServer(IEnumerable<KeyValuePair<string, string>> data, string optional_headers = null)
        {
            try
            {
                client = new HttpClient();
                client.BaseAddress = new Uri(Url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new FormUrlEncodedContent(
                    data
                );

                var result = client.PostAsync(Url, content).Result;
                string resultContent = result.Content.ReadAsStringAsync().Result;
                return resultContent;
            }
            catch (Exception e)
            {
                ExceptionLogging.LogException(e);
                client = new HttpClient();
                return "Failure";
            }
        }

        public string SendMessageNonAsync(string message, string recipientNumber)
        {
            IEnumerable<KeyValuePair<string, string>> data = new[]
            {
                new KeyValuePair<string, string>("Type", "sendparam"),
                new KeyValuePair<string, string>("Username", UserName),
                new KeyValuePair<string, string>("Password", Password),
                new KeyValuePair<string, string>("numto", recipientNumber),
                new KeyValuePair<string, string>("data1", message),
            };

            string result = QuerySmsServerNonAsync(data);
            return result;
        }

        // query API server and return response in object format
        private string QuerySmsServerNonAsync(IEnumerable<KeyValuePair<string, string>> data, string optional_headers = null)
        {
            try
            {
                client = new HttpClient();
                client.BaseAddress = new Uri(Url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new FormUrlEncodedContent(
                    data
                );

                var result = client.PostAsync(Url, content).Result;
                string resultContent = result.Content.ReadAsStringAsync().Result;
                return resultContent;
            }
            catch (Exception e)
            {
                ExceptionLogging.LogException(e);
                client = new HttpClient();
                return "Failure";
            }
        }
    }
}