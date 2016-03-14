using System;
using System.Web.Mvc;
using OnlineHlslCompiler.ViewModels;
using SharpDX.D3DCompiler;

namespace OnlineHlslCompiler.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View(new HomeViewModel
            {
                Code = @"float4 PS() : SV_Target
{
    return float4(1, 0, 0, 1);
}",
                TargetProfile = TargetProfile.ps_4_0,
                EntryPointName = "PS"
            });
        }

        [HttpPost]
        public ActionResult Compile(HomeViewModel model)
        {
            try
            {
                var compilationResult = ShaderBytecode.Compile(
                model.Code, model.EntryPointName,
                model.TargetProfile.ToString());

                var result = new CompilationResultViewModel();
                result.HasErrors = compilationResult.HasErrors;
                result.Message = compilationResult.Message;

                if (!compilationResult.HasErrors)
                    result.Disassembly = compilationResult.Bytecode.Disassemble(DisassemblyFlags.None);

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    HasErrors = true,
                    Message = ex.ToString()
                });
            }
        }
    }
}