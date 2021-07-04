using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Project_QLCHDC.Startup))]
namespace Project_QLCHDC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
