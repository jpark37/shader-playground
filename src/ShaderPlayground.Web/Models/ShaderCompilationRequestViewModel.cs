using System.Collections.Generic;
using ShaderPlayground.Core;

namespace ShaderPlayground.Web.Models
{
    public class ShaderCompilationRequestViewModel
    {
        public string Code { get; set; }

        public string Language { get; set; }

        public CompilationStepViewModel[] CompilationSteps { get; set; }
    }

    public class CompilationStepViewModel
    {
        public string Compiler { get; set; }
        public Dictionary<string, string> Arguments { get; set; }
    }

    public class ShaderCompilerResultViewModel
    {
        public string BinaryOutput { get; }
        public int? SelectedOutputIndex { get; }
        public ShaderCompilerOutput[] Outputs { get; }

        public ShaderCompilerResultViewModel(string binaryOutput, int? selectedOutputIndex, params ShaderCompilerOutput[] outputs)
        {
            BinaryOutput = binaryOutput;
            SelectedOutputIndex = selectedOutputIndex;
            Outputs = outputs;
        }
    }
}
