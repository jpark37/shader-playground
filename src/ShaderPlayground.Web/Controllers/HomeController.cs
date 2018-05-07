using Microsoft.AspNetCore.Mvc;

namespace ShaderPlayground.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
