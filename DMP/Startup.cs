using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DMP.Startup))]
namespace DMP
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
