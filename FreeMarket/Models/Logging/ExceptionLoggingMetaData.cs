using Microsoft.AspNet.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;

namespace FreeMarket.Models
{
    public enum LoggingSeverityLevels
    {
        Transactional,
        Audit,
        Verbose
    };

    [MetadataType(typeof(ExceptionLoggingMetaData))]
    public partial class ExceptionLogging
    {
        public static void LogException(Exception e)
        {
            var currentUserName = HttpContext.Current.User.Identity.GetUserId() ?? "Anonymous";

            ExceptionLogging ex = new ExceptionLogging()
            {
                DateTime = System.DateTime.Now,
                Identity = currentUserName,
                Message = e.Message,
                StackTrace = e.StackTrace
            };

            try
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    db.ExceptionLoggings.Add(ex);
                    db.SaveChanges();
                }
            }
            catch
            {
                // Could not log error
                ex.ExceptionNumber = 999999;
            }
            finally
            {
                System.Diagnostics.Debug.Write(string.Format("Exception swallowed by Filter: {0}", ex));

                if ((ConfigurationManager.AppSettings["notifyDeveloperOfExceptions"]) == "true")
                    NotifyDeveloper(ex);
            }
        }

        public static async void LogExceptionAsync(Exception e)
        {
            var currentUserName = HttpContext.Current.User.Identity.GetUserId() ?? "Anonymous";

            ExceptionLogging ex = new ExceptionLogging()
            {
                DateTime = System.DateTime.Now,
                Identity = currentUserName,
                Message = e.Message,
                StackTrace = e.StackTrace
            };

            try
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    db.ExceptionLoggings.Add(ex);
                    db.SaveChanges();
                }
            }
            catch
            {
                // Could not log error
                ex.ExceptionNumber = 999;
            }
            finally
            {
                if ((ConfigurationManager.AppSettings["notifyDeveloperOfExceptions"]) == "true")
                    await NotifyDeveloperAsync(ex);
            }
        }

        public static async Task<int> NotifyDeveloperAsync(ExceptionLogging ex)
        {
            if (ex != null)
            {
                EmailService email = new EmailService();
                IdentityMessage message = new IdentityMessage();
                message.Destination = ConfigurationManager.AppSettings["supportEmail"];
                message.Body = String.Format("An exception occurred: " + "<tr><th>Number</th><th>Date</th><th>Identity</th><th>Message</th></tr><tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", ex.ExceptionNumber, ex.DateTime, ex.Identity, ex.Message);
                message.Subject = String.Format("Exception {0}", ex.ExceptionNumber);
                await email.SendAsync(message);
            }

            return 0;
        }

        public static int NotifyDeveloper(ExceptionLogging ex)
        {
            if (ex != null)
            {
                EmailService email = new EmailService();
                IdentityMessage message = new IdentityMessage();
                message.Destination = ConfigurationManager.AppSettings["supportEmail"];
                message.Body = String.Format("An exception occurred: " + "<tr><th>Number</th><th>Date</th><th>Identity</th><th>Message</th></tr><tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", ex.ExceptionNumber, ex.DateTime, ex.Identity, ex.Message);
                message.Subject = String.Format("Exception {0}", ex.ExceptionNumber);
                email.SendAsync(message);
            }

            return 0;
        }
    }

    public class ExceptionLoggingMetaData
    {
    }
}