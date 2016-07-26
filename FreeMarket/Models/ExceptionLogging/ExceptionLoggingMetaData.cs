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
        public static async void LogException(Exception e)
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
                    await NotifyDeveloper(ex);
            }
        }

        public static async Task<int> NotifyDeveloper(ExceptionLogging ex)
        {
            if (ex != null)
            {
                EmailService email = new EmailService();
                IdentityMessage message = new IdentityMessage();
                message.Destination = ConfigurationManager.AppSettings["supportEmail"];
                message.Body = String.Format("An exception occurred: {0} : {1} : {2} : {3}", ex.ExceptionNumber, ex.DateTime, ex.Identity, ex.Message);
                message.Subject = String.Format("Exception {0}", ex.ExceptionNumber);
                await email.SendAsync(message);
            }

            return 0;
        }
    }

    public class ExceptionLoggingMetaData
    {
    }
}