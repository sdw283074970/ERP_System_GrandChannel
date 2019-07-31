using ClothResorting.Manager;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ClothResorting.Startup))]
namespace ClothResorting
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR<DefaultConnection>("/testconnection");
            ConfigureAuth(app);
        }
    }
}
