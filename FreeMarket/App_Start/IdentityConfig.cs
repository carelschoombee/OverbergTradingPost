using FreeMarket.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Configuration;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace FreeMarket
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            RestClient client = new RestClient();
            client.BaseUrl = new Uri("https://api.mailgun.net/v3");
            client.Authenticator =
                   new HttpBasicAuthenticator("api",
                                              ConfigurationManager.AppSettings["emailClientApiKey"]);
            RestRequest request = new RestRequest();
            request.AddParameter("domain",
                                ConfigurationManager.AppSettings["emailDomainKey"], ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Mailgun Sandbox <postmaster@sandboxe19f3641c58f4a56b87fbaf89339f2fb.mailgun.org>");
            request.AddParameter("to", message.Destination);
            request.AddParameter("subject", message.Subject);
            string body = EmailService.Borderify(message.Body);
            request.AddParameter("html", string.Format("<html><body><table>{0}<tr><td><br />Thank you for using the &copy; Free Market platform</td></tr><tr><td><br /><img src=\"cid:google.png\"></td></tr></table><body></html>", body));
            request.AddFile("inline", HttpContext.Current.Server.MapPath("~/Content/Images/google.png"));
            request.AddFile("attachment", Path.Combine("files", HttpContext.Current.Server.MapPath("~/Content/Images/google.png")));
            request.Method = Method.POST;

            client.Execute(request);

            return Task.FromResult(0);
        }

        public Task SendAsync(IdentityMessage message, MemoryStream attachment)
        {
            RestClient client = new RestClient();
            client.BaseUrl = new Uri("https://api.mailgun.net/v3");
            client.Authenticator =
                   new HttpBasicAuthenticator("api",
                                              ConfigurationManager.AppSettings["emailClientApiKey"]);
            RestRequest request = new RestRequest();
            request.AddParameter("domain",
                                ConfigurationManager.AppSettings["emailDomainKey"], ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Mailgun Sandbox <postmaster@sandboxe19f3641c58f4a56b87fbaf89339f2fb.mailgun.org>");
            request.AddParameter("to", message.Destination);
            request.AddParameter("subject", message.Subject);
            string body = EmailService.Borderify(message.Body);
            request.AddParameter("html", string.Format("<html><body><table>{0}<tr><td><br />Thank you for using the &copy; Free Market platform</td></tr><tr><td><br /><img src=\"cid:google.png\"></td></tr></table><body></html>", body));
            request.AddFile("inline", HttpContext.Current.Server.MapPath("~/Content/Images/google.png"));
            request.AddFile("attachment", attachment.ToArray(), "Order.pdf");
            request.Method = Method.POST;

            client.Execute(request);

            return Task.FromResult(0);
        }

        public static string Borderify(string html)
        {
            html = html.Replace("<th>", "<th style='border-left: solid 1px black;border-right: solid 1px black;border-top: solid 1px black;border-bottom: solid 1px black'>");
            html = html.Replace("<td>", "<td style='border-left: solid 1px black;border-right: solid 1px black;border-top: solid 1px black;border-bottom: solid 1px black'>");
            return html;
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
