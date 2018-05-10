using System;
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
                var compilationResult = Compiler.Compile(
                    model.Code, 
                    model.Language, 
                    model.Compiler, 
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
