using ClothResorting;
using ClothResorting.Manager;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace ClothResorting
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //app.MapSignalR<DefaultConnection>("/testconnection");
            app.MapSignalR();
            ConfigureAuth(app);
        }
    }
}
