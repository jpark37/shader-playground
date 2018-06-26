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
        public bool Success { get; }
        public string BinaryOutput { get; }
        public string OutputSize { get; }
        public int? SelectedOutputIndex { get; }
        public ShaderCompilerOutput[] Outputs { get; }

        public ShaderCompilerResultViewModel(bool success, string binaryOutput, string outputSize, int? selectedOutputIndex, params ShaderCompilerOutput[] outputs)
        {
            Success = success;
            BinaryOutput = binaryOutput;
            OutputSize = outputSize;
            SelectedOutputIndex = selectedOutputIndex;
            Outputs = outputs;
        }
    }

    public class ShaderCompilerResultsViewModel
    {
        public ShaderCompilerResultViewModel[] Results { get; }

        public ShaderCompilerResultsViewModel(ShaderCompilerResultViewModel[] results)
        {
            Results = results;
        }
    }
}
