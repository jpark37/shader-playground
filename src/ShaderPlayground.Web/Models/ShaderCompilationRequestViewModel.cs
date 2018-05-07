using System.Collections.Generic;

namespace ShaderPlayground.Web.Models
{
    public class ShaderCompilationRequestViewModel
    {
        public string Code { get; set; }

        public string Language { get; set; }
        public string Compiler { get; set; }

        public Dictionary<string, string> Arguments { get; set; }
    }
}
