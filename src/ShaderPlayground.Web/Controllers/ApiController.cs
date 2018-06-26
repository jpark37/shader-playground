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
                var compilationResults = Compiler.Compile(
                    new ShaderCode(model.Language, model.Code),
                    model.CompilationSteps
                        .Select(x => new CompilationStep(x.Compiler, x.Arguments))
                        .ToArray());

                var viewModels = compilationResults
                    .Select(x =>
                    {
                        var binaryOutput = x.PipeableOutput?.Binary != null
                            ? Convert.ToBase64String(x.PipeableOutput.Binary)
                            : null;

                        string outputSize = null;
                        if (x.PipeableOutput?.Binary != null)
                        {
                            outputSize = x.PipeableOutput.Binary.Length + " bytes";
                        }
                        else if (x.PipeableOutput?.Text != null)
                        {
                            outputSize = x.PipeableOutput.Text.Length + " characters";
                        }

                        return new ShaderCompilerResultViewModel(
                            x.Success,
                            binaryOutput,
                            outputSize,
                            x.SelectedOutputIndex,
                            x.Outputs);
                    })
                    .ToArray();

                return Json(new ShaderCompilerResultsViewModel(viewModels));
            }
            catch (Exception ex)
            {
                return Json(new ShaderCompilerResultsViewModel(
                    new[]
                    {
                        new ShaderCompilerResultViewModel(
                            false,
                            null,
                            null,
                            0,
                            new ShaderCompilerOutput(
                                "Site error",
                                null,
                                ex.ToString()))
                    }));
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
