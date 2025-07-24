using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(salesexpensetracker.Startup))]
namespace salesexpensetracker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
