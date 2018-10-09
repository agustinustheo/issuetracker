using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MVC_Final_Project.Startup))]
namespace MVC_Final_Project
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
