using System.Web.Mvc;
using SharpDX.D3DCompiler;

namespace OnlineHlslCompiler.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var compilationResult = ShaderBytecode.Compile(@"float4 PS() : SV_Target { return float4(1, 0, 0, 1); }", "PS", "ps_4_0");
            if (!compilationResult.HasErrors)
            {
                return new ContentResult { Content = compilationResult.Bytecode.Disassemble(DisassemblyFlags.None)};
            }
            else
            {
                return new ContentResult { Content = compilationResult.Message };
            }

            return View();
        }
    }
}