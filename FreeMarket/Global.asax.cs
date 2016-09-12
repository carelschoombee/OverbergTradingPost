using FreeMarket.Models;
using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace FreeMarket
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Roles.ApplicationName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            Membership.ApplicationName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
            ModelBinders.Binders.Add(typeof(DateTime), new DateTimeModelBinder());
            ModelBinders.Binders.Add(typeof(DateTime?), new DateTimeModelBinder());

            try
            {
                if (Roles.RoleExists("Administrator") == false)
                    Roles.CreateRole("Administrator");

                if (!Roles.IsUserInRole("carelschoombee@gmail.com", "Administrator"))
                    Roles.AddUserToRole("carelschoombee@gmail.com", "Administrator");
            }
            catch (Exception e)
            {

            }
        }
    }
}
