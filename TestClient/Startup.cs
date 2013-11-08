using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TestClient.Startup))]
namespace TestClient
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
