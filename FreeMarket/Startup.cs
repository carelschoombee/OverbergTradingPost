using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FreeMarket.Startup))]
namespace FreeMarket
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
