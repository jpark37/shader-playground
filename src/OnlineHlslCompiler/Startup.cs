using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OnlineHlslCompiler.Startup))]
namespace OnlineHlslCompiler
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
