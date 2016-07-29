using System.Web.Mvc;

namespace FreeMarket.Models
{
    public class FreeMarketErrorHandler : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled) return;

            if (filterContext.Exception != null) ExceptionLogging.LogException(filterContext.Exception);

            filterContext.Result = new ViewResult { ViewName = View };

            filterContext.ExceptionHandled = true;
        }
    }
}