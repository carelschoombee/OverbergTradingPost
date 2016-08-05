using System;
using System.ComponentModel.DataAnnotations;

namespace FreeMarket.Models
{
    [MetadataType(typeof(AuditUserMetaData))]
    public partial class AuditUser
    {
        public static void LogAudit(short action, string parameters, string userId = null)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                string user = userId ?? "Anonymous";

                if (ExceptionLogging.AuditLoggingEnabled())
                {
                    AuditUser audit = new AuditUser()
                    {
                        Identity = user,
                        DateTime = DateTime.Now,
                        Action = action,
                        Parameters = parameters
                    };

                    db.AuditUsers.Add(audit);
                    db.SaveChanges();
                }
            }
        }
    }

    public class AuditUserMetaData
    {

    }
}