using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ShieldPortal.Startup))]
namespace ShieldPortal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            ConfigureAuth(app);            
        }
    }
}
