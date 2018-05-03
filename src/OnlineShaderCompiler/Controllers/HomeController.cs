using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OnlineShaderCompiler.Models;

namespace OnlineShaderCompiler.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(new HomeViewModel
            {
                Code = "",
                //Compiler = Compiler.NewCompiler,
                //TargetProfile = TargetProfile.ps_6_0,
                //EntryPointName = "PSMain"
            });
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
