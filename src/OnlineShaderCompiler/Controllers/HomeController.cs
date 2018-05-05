using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OnlineShaderCompiler.Framework;
using OnlineShaderCompiler.Models;

namespace OnlineShaderCompiler.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(new HomeViewModel
            {
                Languages = ShaderLanguages.All
            });
        }

        [HttpPost]
        public ActionResult Compile([FromBody] ShaderProcessorRequestViewModel model)
        {
            try
            {
                var language = ShaderLanguages.All.First(x => x.Name == model.Language);
                var processor = language.Processors.First(x => x.Name == model.Processor);

                var compilationResult = processor.Process(
                    model.Code, 
                    model.Arguments);

                return Json(compilationResult);
            }
            catch (Exception ex)
            {
                return Json(new ShaderProcessorResult(
                    new ShaderProcessorOutput(
                        "Site error",
                        null, 
                        ex.ToString())));
            }
        }
    }
}
