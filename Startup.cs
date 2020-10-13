using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Lagsoba94.Startup))]
namespace Lagsoba94
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
