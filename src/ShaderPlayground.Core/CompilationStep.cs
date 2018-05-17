using System.Collections.Generic;

namespace ShaderPlayground.Core
{
    public sealed class CompilationStep
    {
        public string CompilerName { get; }
        public Dictionary<string, string> Arguments { get; }

        public CompilationStep(string compilerName, Dictionary<string, string> arguments)
        {
            CompilerName = compilerName;
            Arguments = arguments;
        }
    }
}
