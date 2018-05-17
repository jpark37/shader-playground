using System;
using System.Linq;
using System.Threading.Tasks;
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
                    new ShaderCode(model.Language, model.Code),
                    model.CompilationSteps
                        .Select(x => new CompilationStep(x.Compiler, x.Arguments))
                        .ToArray());

                var viewModel = new ShaderCompilerResultViewModel(
                    compilationResult.SelectedOutputIndex, 
                    compilationResult.Outputs);

                return Json(viewModel);
            }
            catch (Exception ex)
            {
                return Json(new ShaderCompilerResultViewModel(
                    0,
                    new ShaderCompilerOutput(
                        "Site error",
                        null,
                        ex.ToString())));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateGist([FromBody] ShaderCompilationRequestViewModel model)
        {
            var gistId = await GitHubUtility.CreateGistId(model);
            return Json(gistId);
        }
    }
}
