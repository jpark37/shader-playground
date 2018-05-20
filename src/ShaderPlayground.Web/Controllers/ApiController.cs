using System;
using System.Linq;
using System.Text;
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

                var binaryOutput = compilationResult.PipeableOutput?.Binary != null
                    ? Convert.ToBase64String(compilationResult.PipeableOutput.Binary)
                    : null;

                var viewModel = new ShaderCompilerResultViewModel(
                    binaryOutput,
                    compilationResult.SelectedOutputIndex, 
                    compilationResult.Outputs);

                return Json(viewModel);
            }
            catch (Exception ex)
            {
                return Json(new ShaderCompilerResultViewModel(
                    null,
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
