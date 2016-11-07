using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FreeMarket.Models
{
    [MetadataType(typeof(AuditUserMetaData))]
    public partial class AuditUser
    {
        public string ActionDescription { get; set; }

        public static List<AuditUser> GetAudits(string filter)
        {
            List<AuditUser> audits = new List<AuditUser>();

            if (!string.IsNullOrEmpty(filter))
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    audits = db.FilterAuditUser(filter).Select(c => new AuditUser
                    {
                        Action = (short)c.Action,
                        DateTime = (DateTime)c.DateTime,
                        Identity = c.Identity,
                        ActionNumber = (long)c.ActionNumber,
                        Parameters = c.Parameters,
                        ActionDescription = c.ActionDescription

                    })
                    .ToList();
                }
            }
            else
            {
                audits = new List<AuditUser>();
            }

            return audits;
        }

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