using System.Collections.Generic;
using OnlineShaderCompiler.Framework;

namespace OnlineShaderCompiler.Models
{
    public class HomeViewModel
    {
        public IShaderLanguage[] Languages { get; set; }
    }

    public class ShaderProcessorRequestViewModel
    {
        public string Code { get; set; }

        public string Language { get; set; }
        public string Processor { get; set; }

        public Dictionary<string, string> Arguments { get; set; }
    }
}
