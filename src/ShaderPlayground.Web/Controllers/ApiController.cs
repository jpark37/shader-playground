using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ShaderPlayground.Core;
using ShaderPlayground.Web.Models;

namespace ShaderPlayground.Web.Controllers
{
    public class ApiController : Controller
    {
        [HttpPost]
        public IActionResult Compile([FromBody] ShaderCompilationRequestViewModel model)
        {
            try
            {
                var language = ShaderLanguages.All.First(x => x.Name == model.Language);
                var processor = language.Compilers.First(x => x.Name == model.Compiler);

                var compilationResult = processor.Compile(
                    model.Code, 
                    model.Arguments);

                return Json(compilationResult);
            }
            catch (Exception ex)
            {
                return Json(new ShaderCompilerResult(
                    0,
                    new ShaderCompilerOutput(
                        "Site error",
                        null, 
                        ex.ToString())));
            }
        }
    }
}
